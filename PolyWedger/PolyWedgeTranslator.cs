using System.Numerics;
using HelixToolkit.SharpDX;

namespace PolyWedger;

public static class PolyWedgeTranslator
{
    public static Geometry3D.Triangle[] ProcessModelsPolygons(Geometry3D.Triangle[] polygons)
    {
        var result = new List<Geometry3D.Triangle>();
        foreach (var polygon in polygons)
        {
            var bisected = BisectPolygon(polygon);
            result.AddRange(bisected);
        }

        return result.ToArray();
    }

    private static Wedge TranslatePolygonToWedge(Geometry3D.Triangle polygon)
    {
        // Find center point of triangle
        var cX = (polygon.P0.X + polygon.P1.X + polygon.P2.X) / 3;
        var cY = (polygon.P0.Y + polygon.P1.Y + polygon.P2.Y) / 3;
        var cZ = (polygon.P0.Z + polygon.P1.Z + polygon.P2.Z) / 3;
        var center = new Vector3(cX, cY, cZ);

        // Find polygon normal
        var a = polygon.P1 - polygon.P0;
        var b = polygon.P2 - polygon.P0;
        var normal = Vector3.Cross(a, b);
        normal = Vector3.Normalize(normal);

        // Decide on reference axis
        var q = FromToRotation(Vector3.UnitZ, normal);      // quaternion that rotates +Z to the polygon normal
        var e = ToEulerAngles(q) * (180f / (float)Math.PI); // convert radians to degrees

        // Wedge rotation:Vector3 is made from normal
        // Wedge position is just center point
        // Wedge scale is unknown for now

        return new Wedge()
        {
            Pos = Vector3.Zero,
            Rot = Vector3.Zero,
            Scale = Vector3.One
        };
    }
    
    public static Quaternion FromToRotation(Vector3 from, Vector3 to)
    {
        var f = Vector3.Normalize(from);
        var t = Vector3.Normalize(to);
        float dot = Vector3.Dot(f, t);
        const float eps = 1e-6f;

        if (dot > 1f - eps)
            return Quaternion.Identity;

        if (dot < -1f + eps)
        {
            // 180 degree rotation: find any orthogonal axis
            var ortho = Vector3.Cross(Vector3.UnitX, f);
            if (ortho.LengthSquared() < eps)
                ortho = Vector3.Cross(Vector3.UnitY, f);
            ortho = Vector3.Normalize(ortho);
            return Quaternion.CreateFromAxisAngle(ortho, (float)Math.PI);
        }

        // Numerically stable quaternion from two vectors:
        // q = [cross(f,t), 1 + dot(f,t)] then normalize.
        var cross = Vector3.Cross(f, t);
        var q = new Quaternion(cross.X, cross.Y, cross.Z, 1f + dot);

        // Normalize safely
        var lenSq = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
        if (lenSq > 0f)
        {
            var inv = 1f / (float)Math.Sqrt(lenSq);
            q.X *= inv; q.Y *= inv; q.Z *= inv; q.W *= inv;
        }

        return q;
    }

    public static Vector3 ToEulerAngles(Quaternion q)
    {
        // Normalize quaternion first to reduce floating-point drift
        var mag = (float)Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
        if (mag > 0f)
        {
            q.X /= mag; q.Y /= mag; q.Z /= mag; q.W /= mag;
        }

        Vector3 angles = new();

        // roll (x)
        double sinrCosp = 2.0 * (q.W * q.X + q.Y * q.Z);
        double cosrCosp = 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
        angles.X = (float)Math.Atan2(sinrCosp, cosrCosp);

        // pitch (y) with clamping to avoid NaN
        double sinp = 2.0 * (q.W * q.Y - q.Z * q.X);
        sinp = Math.Max(-1.0, Math.Min(1.0, sinp));
        if (Math.Abs(sinp) >= 1.0)
        {
            angles.Y = (float)(Math.PI / 2.0 * Math.Sign(sinp)); // use 90 degrees if out of range
        }
        else
        {
            angles.Y = (float)Math.Asin(sinp);
        }

        // yaw (z)
        double sinyCosp = 2.0 * (q.W * q.Z + q.X * q.Y);
        double cosyCosp = 1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
        angles.Z = (float)Math.Atan2(sinyCosp, cosyCosp);

        return angles;
    }

    // Perhaps we can run this as a compute shader and use buffers for faster processing on large models.
    // Okay never mind that, this is already mad fast for what it does.
    // 190ms for 1mio polygons on my PC is good enough, especially when max polys are 400k.
    // Though maybe it will be good when we translate the polygons into wedges as well.
    private static Geometry3D.Triangle[] BisectPolygon(Geometry3D.Triangle polygon)
    {
        // Put vertices into an array for easier indexing
        var pts = new[] { polygon.P0, polygon.P1, polygon.P2 };

        // Describe the 3 edges: (startIndex, endIndex, oppositeIndex)
        var edges = new (int i, int j, int k)[]
        {
            (0, 1, 2), // AB, opposite C
            (1, 2, 0), // BC, opposite A
            (2, 0, 1) // CA, opposite B
        };

        // Find the longest edge
        var best = edges[0];
        var bestLen = Vector3.Distance(pts[best.i], pts[best.j]);

        for (int idx = 1; idx < edges.Length; idx++)
        {
            var e = edges[idx];
            var len = Vector3.Distance(pts[e.i], pts[e.j]);
            if (len >= bestLen)
            {
                best = e;
                bestLen = len;
            }
        }

        // Now compute u, v, w, d for the chosen edge
        var a = pts[best.i]; // start of longest edge
        var b = pts[best.j]; // end of longest edge
        var c = pts[best.k]; // opposite vertex

        var u = a - c;
        var v = b - c;
        var w = b - a;

        if (Vector3.Dot(u, v) == 0)
        {
            return [polygon];
        }

        var scalarProjection = -Vector3.Dot(u, w) / Vector3.Dot(w, w);
        var d = a + scalarProjection * w;

        // Create two new right-angle triangles: ACD and BCD
        var tri1 = new Geometry3D.Triangle
        {
            P0 = a,
            P1 = c,
            P2 = d
        };
        var tri2 = new Geometry3D.Triangle
        {
            P0 = b,
            P1 = c,
            P2 = d
        };

        return [tri1, tri2];
    }
}
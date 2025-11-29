using System.Numerics;
using HelixToolkit.SharpDX;

namespace PolyWedger;

public static class PolyWedgeTranslator
{
    public static Wedge[] ProcessModelsPolygons(Geometry3D.Triangle[] polygons)
    {
        List<Wedge> result = [];
        foreach (Geometry3D.Triangle polygon in polygons)
        {
            Geometry3D.Triangle[] bisected = BisectPolygon(polygon);
            result.AddRange(bisected.Select(TranslatePolygonToWedge));
        }

        return result.ToArray();
    }

    public static Wedge TranslatePolygonToWedge(Geometry3D.Triangle polygon)
    {
        // Find center point of triangle
        Vector3 c = new(
            (polygon.P0.X + polygon.P1.X + polygon.P2.X) / 3f,
            (polygon.P0.Y + polygon.P1.Y + polygon.P2.Y) / 3f,
            (polygon.P0.Z + polygon.P1.Z + polygon.P2.Z) / 3f
        );

        // Find polygon normal
        Vector3 a = polygon.P1 - polygon.P0;
        Vector3 b = polygon.P2 - polygon.P0;
        Vector3 n = Vector3.Normalize(Vector3.Cross(a, b));

        // Wedge rotation:Vector3 is made from normal
        Quaternion q = QuaternionUtils.FromToRotation(Vector3.UnitY, n);

        // Compute scales using helper
        (float scaleY, float scaleZ) = ComputeLocalScales(polygon, c, Quaternion.Inverse(q));

        return new Wedge()
        {
            Pos = c,
            Rot = QuaternionUtils.QuaternionToEulerDegrees(q),
            Scale = new Vector3(0f, scaleY, scaleZ),
        };
    }
    
    public static (float scaleY, float scaleZ) ComputeLocalScales(Geometry3D.Triangle polygon, Vector3 center, Quaternion inverse)
    {
        Vector3 lp0 = Vector3.Transform(polygon.P0 - center, inverse);
        Vector3 lp1 = Vector3.Transform(polygon.P1 - center, inverse);
        Vector3 lp2 = Vector3.Transform(polygon.P2 - center, inverse);

        float minY = MathF.Min(lp0.Y, MathF.Min(lp1.Y, lp2.Y));
        float maxY = MathF.Max(lp0.Y, MathF.Max(lp1.Y, lp2.Y));
        float minZ = MathF.Min(lp0.Z, MathF.Min(lp1.Z, lp2.Z));
        float maxZ = MathF.Max(lp0.Z, MathF.Max(lp1.Z, lp2.Z));

        return (maxY - minY, maxZ - minZ);
    }

    // Perhaps we can run this as a compute shader and use buffers for faster processing on large models.
    // Okay never mind that, this is already mad fast for what it does.
    // 190ms for 1mio polygons on my PC is good enough, especially when max polys are 400k.
    // Though maybe it will be good when we translate the polygons into wedges as well.
    public static Geometry3D.Triangle[] BisectPolygon(Geometry3D.Triangle polygon)
    {
        // Put vertices into an array for easier indexing
        Vector3[] pts = [polygon.P0, polygon.P1, polygon.P2];

        // Describe the 3 edges: (startIndex, endIndex, oppositeIndex)
        (int i, int j, int k)[] edges =
        [
            (0, 1, 2), // AB, opposite C
            (1, 2, 0), // BC, opposite A
            (2, 0, 1) // CA, opposite B
        ];

        // Find the longest edge
        (int i, int j, int k) best = edges[0];
        float bestLen = Vector3.Distance(pts[best.i], pts[best.j]);

        for (int idx = 1; idx < edges.Length; idx++)
        {
            (int i, int j, int k) e = edges[idx];
            float len = Vector3.Distance(pts[e.i], pts[e.j]);
            if (!(len >= bestLen)) continue;
            best = e;
            bestLen = len;
        }

        // Now compute u, v, w, d for the chosen edge
        Vector3 a = pts[best.i]; // start of longest edge
        Vector3 b = pts[best.j]; // end of longest edge
        Vector3 c = pts[best.k]; // opposite vertex

        Vector3 u = a - c;
        Vector3 v = b - c;
        Vector3 w = b - a;

        if (Vector3.Dot(u, v) == 0)
        {
            return [polygon];
        }

        float scalarProjection = -Vector3.Dot(u, w) / Vector3.Dot(w, w);
        Vector3 d = a + scalarProjection * w;

        // Create two new right-angle triangles: ACD and BCD
        Geometry3D.Triangle tri1 = new Geometry3D.Triangle
        {
            P0 = a,
            P1 = c,
            P2 = d
        };
        Geometry3D.Triangle tri2 = new Geometry3D.Triangle
        {
            P0 = b,
            P1 = c,
            P2 = d
        };

        return [tri1, tri2];
    }
}
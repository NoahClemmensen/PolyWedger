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
        var cX = (polygon.P0.X+polygon.P1.X+polygon.P2.X)/3;
        var cY = (polygon.P0.Y+polygon.P1.Y+polygon.P2.Y)/3;
        var cZ = (polygon.P0.Z+polygon.P1.Z+polygon.P2.Z)/3;
        var center = new Vector3(cX, cY, cZ);
        
        // Find polygon normal
        var a = polygon.P1 - polygon.P0;
        var b = polygon.P2 - polygon.P0;
        var normal = Vector3.Cross(a, b);
        normal = Vector3.Normalize(normal);
        
        // Decide on reference axis
        var worldRef = Vector3.UnitY;
        var d = float.Abs(Vector3.Dot(worldRef, normal));
        if (d > 0.99f)
        {
            worldRef = Vector3.UnitX;
        }
        
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
            (2, 0, 1)  // CA, opposite B
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
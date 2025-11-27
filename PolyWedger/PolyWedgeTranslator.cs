using System.Numerics;
using HelixToolkit.SharpDX;

namespace PolyWedger;

public static class PolyWedgeTranslator
{
    // TODO: Perhaps we can run this as a compute shader and use buffers for faster processing on large models.
    public static Geometry3D.Triangle[] MakeRightAnglePolys(Geometry3D.Triangle polygon)
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
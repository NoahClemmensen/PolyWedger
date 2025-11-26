using HelixToolkit.SharpDX;
using HelixToolkit.SharpDX.Model.Scene;

namespace PolyWedger;

public static class ModelGeometryExtractor
{
    public static List<Geometry3D> GetGeometryFromModel(GroupNode group)
    {
        var geometries = new List<Geometry3D>();
        
        foreach (var node in group.Items)
        {
            if (node is not MeshNode mesh) continue; // A mesh is the actual geometry data.
            var geometry = mesh.Geometry;

            if (geometry == null)
            {
                Console.WriteLine($"Mesh ({node.Name}) has no geometry.");
                continue;
            }
                
            geometries.Add(geometry);
        }
        
        return geometries;
    }

    public static Geometry3D.Triangle[]? GetPolygonsFromGeometry(Geometry3D geometry)
    {
        var indices = geometry.Indices;
        if (indices == null) return null;

        var positions = geometry.Positions;
        if (positions == null) return null;

        var polygons = new Geometry3D.Triangle[indices.Count / 3];

        for (var i = 0; i < indices.Count; i += 3)
        {
            // TODO: Figure out what info we need from each polygon. Then make a custom struct and return that instead.
            // This could possibly include positions, rotations(?), idk.
            var polygon = new Geometry3D.Triangle
            {
                P0 = positions[indices[i]],
                P1 = positions[indices[i + 1]],
                P2 = positions[indices[i + 2]]
            };
            polygons[i / 3] = polygon;
        }

        return polygons;
    }
}
using System.Numerics;
using HelixToolkit.SharpDX;
using HelixToolkit.SharpDX.Assimp;
using HelixToolkit.SharpDX.Model.Scene;

namespace PolyWedger;

internal abstract class Program
{
    private static readonly Importer Importer = new();
    private const string Path = @"C:\Users\noahc\RiderProjects\PolyWedger\Models\Monke.obj";

    private static void Main(string[] args)
    {
        TestPolyWedgeTranslator();
    }
    
    private static void TestPolyWedgeTranslator()
    {
        var triangle = new Geometry3D.Triangle
        {
            P0 = new Vector3(2, 0, 0),
            P1 = new Vector3(1, 0, 0),
            P2 = new Vector3(0, 0, 2)
        };

        var result = PolyWedgeTranslator.MakeRightAnglePolys(triangle);
        Console.WriteLine(result.ToString());
    }

    private static void CountPolygons()
    {
        // TODO: Check whether the file exists and is valid file extension and so on.
        // Get file path from args or user input in future.
        // Check if .pwdg file given, then deserialize instead of import and translation.
        
        var model = ImportModel(Path);
        if (model == null) return;
        
        var modelGeometries = ModelGeometryExtractor.GetGeometryFromModel(model);
        if (modelGeometries.Count <= 0) return;
        
        var polygons = ModelGeometryExtractor.GetPolygonsFromGeometry(modelGeometries[0]);
        
        Console.WriteLine(polygons != null
            ? $"Model has {polygons.Length} polygons."
            : "No polygons found in geometry.");
    }

    private static GroupNode? ImportModel(string filename)
    {
        var scene = Importer.Load(filename);

        if (scene != null) return scene.Root.Items.OfType<GroupNode>().FirstOrDefault();
        
        Console.WriteLine("Failed to load model.");
        return null;

    }
}
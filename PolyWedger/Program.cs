using HelixToolkit.SharpDX.Assimp;
using HelixToolkit.SharpDX.Model.Scene;

namespace PolyWedger;

internal abstract class Program
{
    private static readonly Importer Importer = new();

    private static void Main(string[] args)
    {
        const string path = @"C:\Users\noahc\RiderProjects\PolyWedger\Models\Monke.obj";
        // TODO: Check weather the file exists and is valid file extension and so on.
        
        var model = ImportModel(path);
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
using System.Diagnostics;
using System.Numerics;
using HelixToolkit.SharpDX;
using HelixToolkit.SharpDX.Assimp;
using HelixToolkit.SharpDX.Model.Scene;

namespace PolyWedger;

internal abstract class Program
{
    private static readonly Importer Importer = new();
    private const string Path = @"C:\Users\noahc\RiderProjects\PolyWedger\Models\Monke.obj";
    private const string HighPolyPath = @"C:\Users\noahc\RiderProjects\PolyWedger\Models\High_poly_monke.obj";
    private const string MioPolyPath = @"C:\Users\noahc\RiderProjects\PolyWedger\Models\mio_monke.obj";

    private static void Main(string[] args)
    {
        Console.WriteLine("Starting polygon count...");
        Stopwatch sw = Stopwatch.StartNew();
        CountPolygons();
        sw.Stop();
        Console.WriteLine($"Polygon count completed in {sw.ElapsedMilliseconds} ms.");
    }

    private static void CountPolygons()
    {
        // TODO: Check whether the file exists and is valid file extension and so on.
        // Get file path from args or user input in future.
        // Check if .pwdg file given, then deserialize instead of import and translation.

        GroupNode? model = ImportModel(Path);
        if (model == null) return;

        List<Geometry3D> modelGeometries = ModelGeometryExtractor.GetGeometryFromModel(model);
        if (modelGeometries.Count <= 0) return;

        Geometry3D.Triangle[]? polygons = ModelGeometryExtractor.GetPolygonsFromGeometry(modelGeometries[0]);

        Console.WriteLine(polygons != null
            ? $"Model has {polygons.Length} polygons."
            : "No polygons found in geometry.");
    }

    private static GroupNode? ImportModel(string filename)
    {
        HelixToolkitScene? scene = Importer.Load(filename);

        if (scene != null) return scene.Root.Items.OfType<GroupNode>().FirstOrDefault();

        Console.WriteLine("Failed to load model.");
        return null;
    }
}
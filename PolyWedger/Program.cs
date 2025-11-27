using System.Diagnostics;
using System.Numerics;
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
        //TestPolyWedgeTranslator();
        
        Vector3 v = new() { X = 1, Y = 0, Z = 0 };

        var q = PolyWedgeTranslator.FromToRotation(Vector3.UnitZ, v);      // quaternion that rotates +Z to the polygon normal
        var n = PolyWedgeTranslator.ToEulerAngles(q) * (180f / (float)Math.PI); // convert radians to degrees

        Console.WriteLine("Angles: " + n);
    }

    private static void TestPolyWedgeTranslator()
    {
        // var triangle = new Geometry3D.Triangle
        // {
        //     P0 = new Vector3(2, 0, 0),
        //     P1 = new Vector3(1, 0, 0),
        //     P2 = new Vector3(0, 0, 2)
        // };
        //
        // var result = PolyWedgeTranslator.BisectPolygon(triangle);
        // Console.WriteLine(result.ToString());


        var model = ImportModel(MioPolyPath);
        if (model == null) return;

        var modelGeometries = ModelGeometryExtractor.GetGeometryFromModel(model);
        if (modelGeometries.Count <= 0) return;

        var polygons = ModelGeometryExtractor.GetPolygonsFromGeometry(modelGeometries[0]);
        if (polygons == null) return;
        Console.WriteLine($"Polygons count before processing: {polygons.Length}");

        // Start timing
        var stopwatch = Stopwatch.StartNew();

        var processedPolygons = PolyWedgeTranslator.ProcessModelsPolygons(polygons!);

        // Stop timing
        stopwatch.Stop();

        Console.WriteLine($"Processed {processedPolygons.Length} polygons.");
        Console.WriteLine($"Processing took {stopwatch.ElapsedMilliseconds} ms.");

        var diff = processedPolygons.Length - polygons.Length;
        Console.WriteLine($"Polygon count changed by {diff}.");
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
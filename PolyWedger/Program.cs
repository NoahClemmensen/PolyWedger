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
        // TestQuaternionUtils();
        // CountPolygons();
        //TestPolyWedgeTranslator();
    }
    
    private static void TestQuaternionUtils()
    {
        (Vector3 v, string note)[] tests =
        [
            (new Vector3(0, 1, 0), "target +Y -> expect (0, 0, 0)"),
            (new Vector3(0, 0, 1), "target +Z -> expect (90, 0, 0)"),
            (new Vector3(0, -1, 0), "target -Y -> expect equivalent to (180, 0, 0)"),
            (new Vector3(1, 0, 0), "target +X -> expect (0, 0, -90) or (0,0,270)"),
            (new Vector3(-1, 0, 0), "target -X -> expect (0, 0, 90)"),
            (Vector3.Normalize(new Vector3(1, 0, 1)), "diag +X+Z"),
            (Vector3.Normalize(new Vector3(-1, 0, 1)), "diag -X+Z"),
            (Vector3.Normalize(new Vector3(0, 1, 1)), "diag +Y+Z (mostly tilt)"),
            (Vector3.Normalize(new Vector3(0, 1, -1)), "diag +Y-Z"),
            (Vector3.Normalize(new Vector3(1, 1, 1)), "space diagonal")
        ];

        
        foreach ((Vector3 v, string note) in tests)
        {
            Vector3 e = QuaternionUtils.GetNormalizedEulerAngles(v);
            Console.WriteLine($"v = {v} -> angles (X,Y,Z) = {e}  // {note}");
        }
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


        GroupNode? model = ImportModel(MioPolyPath);
        if (model == null) return;

        List<Geometry3D> modelGeometries = ModelGeometryExtractor.GetGeometryFromModel(model);
        if (modelGeometries.Count <= 0) return;

        Geometry3D.Triangle[]? polygons = ModelGeometryExtractor.GetPolygonsFromGeometry(modelGeometries[0]);
        if (polygons == null) return;
        Console.WriteLine($"Polygons count before processing: {polygons.Length}");

        // Start timing
        Stopwatch stopwatch = Stopwatch.StartNew();

        Geometry3D.Triangle[] processedPolygons = PolyWedgeTranslator.ProcessModelsPolygons(polygons!);

        // Stop timing
        stopwatch.Stop();

        Console.WriteLine($"Processed {processedPolygons.Length} polygons.");
        Console.WriteLine($"Processing took {stopwatch.ElapsedMilliseconds} ms.");

        int diff = processedPolygons.Length - polygons.Length;
        Console.WriteLine($"Polygon count changed by {diff}.");
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
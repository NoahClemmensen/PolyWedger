using System;
using System.Linq;
using System.Numerics;
using HelixToolkit.SharpDX;
using NUnit.Framework;
using PolyWedger;

namespace Tests
{
    [TestFixture]
    public class PolyWedgeTranslatorTest
    {
        private const float Eps = 1e-5f;

        [Test]
        public void ProcessModelsPolygons_NonRightTriangle_ReturnsTwoWedges_WithValidScales()
        {
            // Equilateral triangle on XY plane (side length = 1)
            float h = MathF.Sqrt(3f) / 2f;
            Geometry3D.Triangle tri = new()
            {
                P0 = new Vector3(0f, 0f, 0f),
                P1 = new Vector3(1f, 0f, 0f),
                P2 = new Vector3(0.5f, h, 0f)
            };

            Wedge[] wedges = PolyWedgeTranslator.ProcessModelsPolygons([tri]);

            Assert.That(wedges, Has.Length.EqualTo(2), "Expected bisected triangle to produce two wedges.");

            foreach (Wedge w in wedges)
            {
                Assert.Multiple(() =>
                {
                    // TranslatePolygonToWedge forces Scale.X to 0
                    Assert.That(w.Scale.X, Is.EqualTo(0f).Within(Eps), "Scale.X must be zero");
                    Assert.That(w.Scale.Y, Is.GreaterThan(0f), "Scale.Y must be positive");
                    Assert.That(w.Scale.Z, Is.GreaterThan(0f), "Scale.Z must be positive");
                });
            }
        }

        [Test]
        public void ProcessModelsPolygons_RightTriangle_ReturnsSingleWedge()
        {
            // Right triangle (right angle at P0) should not be bisected
            Geometry3D.Triangle tri = new()
            {
                P0 = new Vector3(0f, 0f, 0f),
                P1 = new Vector3(1f, 0f, 0f),
                P2 = new Vector3(0f, 1f, 0f)
            };

            Wedge[] wedges = PolyWedgeTranslator.ProcessModelsPolygons([tri]);

            Assert.That(wedges, Has.Length.EqualTo(1), "Right triangle should produce a single wedge.");
        }

        [Test]
        public void BisectPolygon_NonRightTriangle_ReturnsTwoTriangles_WithFourthPointOnEdge()
        {
            // Equilateral triangle
            float h = MathF.Sqrt(3f) / 2f;
            Geometry3D.Triangle original = new()
            {
                P0 = new Vector3(0f, 0f, 0f),
                P1 = new Vector3(1f, 0f, 0f),
                P2 = new Vector3(0.5f, h, 0f)
            };

            Geometry3D.Triangle[] result = PolyWedgeTranslator.BisectPolygon(original);

            Assert.That(result, Has.Length.EqualTo(2), "Expected bisect to return two triangles");

            // Collect all points from both triangles
            Vector3[] pts =
            [
                result[0].P0, result[0].P1, result[0].P2,
                result[1].P0, result[1].P1, result[1].P2
            ];

            // Determine unique points (within tolerance)
            List<Vector3> unique = pts.Aggregate(new List<Vector3>(), (list, p) =>
            {
                if (!list.Any(q => VectorAlmostEqual(q, p))) list.Add(p);
                return list;
            });

            // Expect 4 unique points: original 3 + the computed intersection
            Assert.That(unique, Has.Count.EqualTo(4), "Expected 4 unique points (original 3 + intersection)");

            Assert.Multiple(() =>
            {
                // Ensure original vertices are present
                Assert.That(unique.Any(p => VectorAlmostEqual(p, original.P0)), Is.True, "Original P0 missing");
                Assert.That(unique.Any(p => VectorAlmostEqual(p, original.P1)), Is.True, "Original P1 missing");
                Assert.That(unique.Any(p => VectorAlmostEqual(p, original.P2)), Is.True, "Original P2 missing");
            });

            // Exactly one new point that is not equal to any original vertex
            int newPoints = unique.Count(p =>
                !VectorAlmostEqual(p, original.P0) &&
                !VectorAlmostEqual(p, original.P1) &&
                !VectorAlmostEqual(p, original.P2)
            );
            Assert.That(newPoints, Is.EqualTo(1), "Expected exactly one new intersection point");
        }

        private static bool VectorAlmostEqual(Vector3 a, Vector3 b, float eps = Eps) =>
            MathF.Abs(a.X - b.X) <= eps && MathF.Abs(a.Y - b.Y) <= eps && MathF.Abs(a.Z - b.Z) <= eps;
    }
}
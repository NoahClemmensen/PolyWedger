 using System.Numerics;
using PolyWedger;

namespace Tests;

[TestFixture]
public class QuaternionUtilsTest
{
    [Test]
    public void GetNormalizedEulerAngles_TargetDirections_ReturnsNormalizedEulerAngles ()
    {
        Vector3[] directions =
        [
            new(0, 1, 0),
            new(0, 0, 1),
            new(0, -1, 0),
            new(1, 0, 0),
            new(-1, 0, 0),
            Vector3.Normalize(new Vector3(1, 0, 1)),
            Vector3.Normalize(new Vector3(-1, 0, 1)),
            Vector3.Normalize(new Vector3(0, 1, 1)),
            Vector3.Normalize(new Vector3(0, 1, -1)),
            Vector3.Normalize(new Vector3(1, 1, 1))
        ];

        const float tol = 1e-3f;

        foreach (Vector3 v in directions)
        {
            Vector3 actual = QuaternionUtils.GetNormalizedEulerAngles(v);

            Quaternion rot = QuaternionUtils.FromToRotation(Vector3.UnitY, v);
            Vector3 expected =
                QuaternionUtils.NormalizeEulerDegrees(QuaternionUtils.ToEulerAngles(rot) * QuaternionUtils.Rad2deg);

            Assert.Multiple(() =>
            {
                Assert.That(actual.X, Is.EqualTo(expected.X).Within(tol), $"v={v} - X");
                Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(tol), $"v={v} - Y");
                Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(tol), $"v={v} - Z");
            });
        }
    }

    [Test]
    public void FromToRotation_RotatesSourceToTarget()
    {
        Vector3[] targets =
        [
            new(0, 1, 0),
            new(0, 0, 1),
            new(1, 0, 0),
            Vector3.Normalize(new Vector3(1, 1, 0)),
            Vector3.Normalize(new Vector3(-1, 1, 1))
        ];

        const float tol = 1e-3f;

        foreach (Vector3 t in targets)
        {
            Quaternion rot = QuaternionUtils.FromToRotation(Vector3.UnitY, t);
            Vector3 rotated = Vector3.Transform(Vector3.UnitY, rot);
            Vector3 rNorm = Vector3.Normalize(rotated);
            Vector3 tNorm = Vector3.Normalize(t);

            Assert.Multiple(() =>
            {
                Assert.That(rNorm.X, Is.EqualTo(tNorm.X).Within(tol), $"target={t} - X");
                Assert.That(rNorm.Y, Is.EqualTo(tNorm.Y).Within(tol), $"target={t} - Y");
                Assert.That(rNorm.Z, Is.EqualTo(tNorm.Z).Within(tol), $"target={t} - Z");
            });
        }
    }

    [Test]
    public void ToEulerAngles_IdentityIsZero()
    {
        Vector3 euler = QuaternionUtils.ToEulerAngles(Quaternion.Identity);
        Vector3 degrees = euler * QuaternionUtils.Rad2deg;
        Vector3 normalized = QuaternionUtils.NormalizeEulerDegrees(degrees);

        const float tol = 1e-6f;
        Assert.Multiple(() =>
        {
            Assert.That(normalized.X, Is.EqualTo(0f).Within(tol));
            Assert.That(normalized.Y, Is.EqualTo(0f).Within(tol));
            Assert.That(normalized.Z, Is.EqualTo(0f).Within(tol));
        });
    }

    [Test]
    public void NormalizeEulerDegrees_WrapsAngles()
    {
        Vector3 input = new(370f, -190f, 540f);
        Vector3 wrapped = QuaternionUtils.NormalizeEulerDegrees(input);

        Vector3 expected = new(10f, 170f, 180f);
        const float tol = 1e-3f;

        Assert.Multiple(() =>
        {
            Assert.That(wrapped.X, Is.EqualTo(expected.X).Within(tol));
            Assert.That(wrapped.Y, Is.EqualTo(expected.Y).Within(tol));
            // Accept both 180 and -180 by comparing the absolute value
            Assert.That(MathF.Abs(wrapped.Z), Is.EqualTo(MathF.Abs(expected.Z)).Within(tol));
        });
    }

    [Test]
    public void Rad2deg_IsCorrect()
    {
        const float tol = 1e-6f;
        Assert.That(QuaternionUtils.Rad2deg, Is.EqualTo(180f / MathF.PI).Within(tol));
    }
}
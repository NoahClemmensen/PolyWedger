using System.Numerics;
using PolyWedger;

namespace Tests
{
    [TestFixture]
    public class SerializerTest
    {
        [Test]
        public void SerializeDeserialize_RoundTrip_PreservesData()
        {
            Wedge[] wedges =
            [
                new Wedge { Pos = new Vector3(1f, 2f, 3f), Rot = new Vector3(0f, 0.5f, 1f), Scale = new Vector3(1f, 1f, 1f) },
                new Wedge { Pos = new Vector3(-1f, 0f, 0.5f), Rot = new Vector3(0.1f, 0.2f, 0.3f), Scale = new Vector3(2f, 2f, 0.5f) }
            ];

            byte[] data = Serializer.Serialize(wedges);
            Wedge[] result = Serializer.Deserialize(data);

            Assert.That(result, Has.Length.EqualTo(wedges.Length));
            for (int i = 0; i < wedges.Length; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(VectorAlmostEqual(wedges[i].Pos, result[i].Pos), Is.True, $"Pos differs at index {i}");
                    Assert.That(VectorAlmostEqual(wedges[i].Rot, result[i].Rot), Is.True, $"Rot differs at index {i}");
                    Assert.That(VectorAlmostEqual(wedges[i].Scale, result[i].Scale), Is.True, $"Scale differs at index {i}");
                });
            }
        }

        [Test]
        public void Deserialize_InvalidSignature_ThrowsInvalidDataException()
        {
            Wedge wedge = new() { Pos = Vector3.Zero, Rot = Vector3.Zero, Scale = Vector3.One };
            byte[] data = Serializer.Serialize([wedge]);

            // Corrupt first byte of signature
            data[0] = (byte)~data[0];

            Assert.Throws<InvalidDataException>(() => Serializer.Deserialize(data));
        }

        [Test]
        public void Deserialize_UnsupportedVersion_ThrowsNotSupportedException()
        {
            Wedge wedge = new() { Pos = Vector3.Zero, Rot = Vector3.Zero, Scale = Vector3.One };
            byte[] data = Serializer.Serialize([wedge]);

            // Version is written after the 4-byte signature (offset 4). Overwrite it with 2.
            byte[] versionBytes = BitConverter.GetBytes(2);
            Array.Copy(versionBytes, 0, data, 4, 4);

            Assert.Throws<NotSupportedException>(() => Serializer.Deserialize(data));
        }

        private static bool VectorAlmostEqual(Vector3 a, Vector3 b, float eps = 1e-5f) =>
            MathF.Abs(a.X - b.X) <= eps && MathF.Abs(a.Y - b.Y) <= eps && MathF.Abs(a.Z - b.Z) <= eps;
    }
}
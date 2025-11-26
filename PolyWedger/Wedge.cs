using System.Numerics;

namespace PolyWedger;

// 36 bytes
// Position (3 * 4 bytes) + Rotation (3 * 4 bytes) + Scale (3 * 4 bytes)
public struct Wedge
{
    public Vector3 Pos;
    public Vector3 Rot;
    public Vector3 Scale;
    // Idk stuff about wedges yet
}
using System.Text;

namespace PolyWedger;

public static class Serializer
{
    // This is where we will serialize the wedge data to a file format.
    // Suggested file extensions: .wedge, .bin, .pwdg
    
    private const int WedgeByteSize = 36;
    private const int HeaderSize = 8 + 4 + 4 + 4; // timestamp (8) + version (4) + wedge count (4) + file type (4)
    private static readonly byte[] FileSignature = Encoding.ASCII.GetBytes("PWDG");

    private static byte[] Serialize(Wedge[] wedges)
    {
        // Size = header size + (number of wedges * byte size of wedge)
        var totalSize = HeaderSize + wedges.Length * WedgeByteSize;

        using var ms = new MemoryStream(totalSize);
        using (var bw = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
        {
            // Header stuff
            bw.Write(FileSignature); // 4 bytes
            bw.Write(1); // Version number (4 bytes)
            bw.Write(wedges.Length); // Number of wedges (4 bytes)
            bw.Write(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); // Timestamp (8 bytes)
            
            // Wedge data
            foreach (var wedge in wedges)
            {
                // Position (3 * 4 bytes)
                bw.Write(wedge.Pos.X); // 4 bytes
                bw.Write(wedge.Pos.Y); // 4 bytes
                bw.Write(wedge.Pos.Z); // 4 bytes
                // Rotation (3 * 4 bytes)
                bw.Write(wedge.Rot.X); // 4 bytes
                bw.Write(wedge.Rot.Y); // 4 bytes
                bw.Write(wedge.Rot.Z); // 4 bytes
                // Scale (3 * 4 bytes)
                bw.Write(wedge.Scale.X); // 4 bytes
                bw.Write(wedge.Scale.Y); // 4 bytes
                bw.Write(wedge.Scale.Z); // 4 bytes
                
                // Total per wedge: 36 bytes
            }
        }

        return ms.ToArray();
    }
    
    private static Wedge[] Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms, Encoding.ASCII, leaveOpen: true);
        
        // Read and validate header
        var signature = br.ReadBytes(4);
        if (!signature.SequenceEqual(FileSignature))
            throw new InvalidDataException("Invalid file signature.");
        
        var version = br.ReadInt32();
        if (version != 1)
            throw new NotSupportedException($"Unsupported version: {version}");
        
        var wedgeCount = br.ReadInt32();
        var timestamp = br.ReadInt64(); // Not used currently
        
        var wedges = new Wedge[wedgeCount];
        for (var i = 0; i < wedgeCount; i++)
        {
            var wedge = new Wedge
            {
                Pos = new System.Numerics.Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                Rot = new System.Numerics.Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                Scale = new System.Numerics.Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle())
            };
            wedges[i] = wedge;
        }

        return wedges;
    }
    
    private static void ReadFile(string path, out byte[] data)
    {
        try
        {
            data = File.ReadAllBytes(path);
        }
        catch (Exception)
        {
            data = [];
            // propagate or handle as needed
            throw;
        }
    }
    
    private static void WriteToFile(string path, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        File.WriteAllBytes(path, data);
    }
}
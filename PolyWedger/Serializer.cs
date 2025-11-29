using System.Text;

namespace PolyWedger;

public static class Serializer
{
    // This is where we will serialize the wedge data to a file format.
    // Suggested file extensions: .wedge, .bin, .pwdg
    
    private const int WedgeByteSize = 36;
    private const int HeaderSize = 8 + 4 + 4 + 4; // timestamp (8) + version (4) + wedge count (4) + file type (4)
    private static readonly byte[] FileSignature = Encoding.ASCII.GetBytes("PWDG");

    public static byte[] Serialize(Wedge[] wedges)
    {
        // Size = header size + (number of wedges * byte size of wedge)
        int totalSize = HeaderSize + wedges.Length * WedgeByteSize;

        using MemoryStream ms = new MemoryStream(totalSize);
        using (BinaryWriter bw = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
        {
            // Header stuff
            bw.Write(FileSignature); // 4 bytes
            bw.Write(1); // Version number (4 bytes)
            bw.Write(wedges.Length); // Number of wedges (4 bytes)
            bw.Write(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); // Timestamp (8 bytes)
            
            // Wedge data
            foreach (Wedge wedge in wedges)
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
    
    public static Wedge[] Deserialize(byte[] data)
    {
        using MemoryStream ms = new MemoryStream(data);
        using BinaryReader br = new BinaryReader(ms, Encoding.ASCII, leaveOpen: true);
        
        // Read and validate header
        byte[] signature = br.ReadBytes(4);
        if (!signature.SequenceEqual(FileSignature))
            throw new InvalidDataException("Invalid file signature.");
        
        int version = br.ReadInt32();
        if (version != 1)
            throw new NotSupportedException($"Unsupported version: {version}");
        
        int wedgeCount = br.ReadInt32();
        long timestamp = br.ReadInt64(); // Not used currently
        
        Wedge[] wedges = new Wedge[wedgeCount];
        for (int i = 0; i < wedgeCount; i++)
        {
            Wedge wedge = new Wedge
            {
                Pos = new System.Numerics.Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                Rot = new System.Numerics.Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                Scale = new System.Numerics.Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle())
            };
            wedges[i] = wedge;
        }

        return wedges;
    }
    
    public static void ReadFile(string path, out byte[] data)
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
    
    public static void WriteToFile(string path, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        File.WriteAllBytes(path, data);
    }
}
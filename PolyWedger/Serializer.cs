namespace PolyWedger;

public class Serializer
{
    // This is where we will serialize the wedge data to a file format.
    // Suggested file extensions: .wedge, .bin, .pwdg
    
    // File will contain:
    // - Header (file type identifier, version number?, timestamp)
    // - Number of wedges
    // - Wedge data (positions, rotations, other relevant info)
    
    // Make sure to:
    // - Implement error handling for file operations.
    // - Use fixed types for binary serialization (e.g., float32, int32).
    // - Have each wedge entry be of fixed byte size for easy reading.

    public static byte[] Serialize(Wedge[] wedges)
    {
        return [];
    }
    
    private static byte[] allocateByteArray()
    {
        // Figure out size of array
        // Size = header size + (number of wedges * byte size of wedge)
        int example = 37632;
        
        return new byte[example];
    }
    
    private static void populateByteArray(byte[] bytes, out byte[] result)
    {
        // Fill byte array with data
        result = bytes;
    }
    
    public static void WriteToFile(string path, byte[] data)
    {
        
    }
}
namespace DarkAges.Library.IO;

public class EpfEntry
{
    public int Id { get; set; }
    public short X1 { get; set; }
    public short Y1 { get; set; }
    public short X2 { get; set; }
    public short Y2 { get; set; }
    public int DataOffset { get; set; }
    public int CompressedDataOffset { get; set; }
    public int CompressedSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string ArchivePath { get; set; } = "";
    public byte[] Data { get; set; }

    public byte[]? GetData()
    {
        if (Data is { Length: > 0 })
        {
            return Data;
        }

        return null;
    }
}
namespace DarkAges.Library.IO;

public class AlbumEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Offset { get; set; }
    public int Size { get; set; }
    public int CompressedSize { get; set; }
    public int Flags { get; set; }
    public byte[] Data { get; set; }
}
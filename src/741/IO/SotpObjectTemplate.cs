namespace DarkAges.Library.IO;

public class SotpObjectTemplate
{
    public int Id { get; set; }
    public int Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Flags { get; set; }
    public byte[] Data { get; set; }
}
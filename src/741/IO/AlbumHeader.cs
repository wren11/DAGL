namespace DarkAges.Library.IO;

public class AlbumHeader
{
    public short ImageCount { get; set; }
    public short PaletteCount { get; set; }
    public int Unknown1 { get; set; }
    public short Width { get; set; }
    public short Height { get; set; }
    public int Unknown2 { get; set; }
}
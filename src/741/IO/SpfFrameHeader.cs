namespace DarkAges.Library.IO;

public class SpfFrameHeader
{
    public short X1 { get; set; }
    public short Y1 { get; set; }
    public short X2 { get; set; }
    public short Y2 { get; set; }
    public int DataOffset { get; set; }
    public int CompressedSize { get; set; }
}
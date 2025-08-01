using System.Text;

namespace DarkAges.Library.IO;

public class AlbumImageEntry
{
    public short Width { get; set; }
    public short Height { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public int DataLength { get; set; }
    public int Offset { get; set; }
}
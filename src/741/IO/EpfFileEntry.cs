namespace DarkAges.Library.IO;

public class EpfFileEntry(EpfArchive archive)
{
    public string Name { get; set; }
    public uint Offset { get; set; }
    public uint CompressedSize { get; set; }
    public uint DecompressedSize { get; set; }
    public uint Flags { get; set; }

    public byte[] GetData()
    {
        return archive.Extract(this);
    }
}
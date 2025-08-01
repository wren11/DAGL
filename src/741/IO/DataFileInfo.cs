namespace DarkAges.Library.IO;

public class DataFileInfo
{
    public string FileType { get; set; } = "";
    public string FileName { get; set; } = "";
    public int RawSize { get; set; }
    public int Header { get; set; }
    public int Version { get; set; }
    public byte[]? ProcessedData { get; set; }
    public List<string> StringTable { get; set; } = [];
    public int StringCount { get; set; }
    public DateTime LoadedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
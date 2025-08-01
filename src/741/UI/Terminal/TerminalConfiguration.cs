namespace DarkAges.Library.UI.Terminal;

/// <summary>
/// Terminal configuration
/// </summary>
public class TerminalConfiguration
{
    public string Name { get; set; }
    public string ServerAddress { get; set; }
    public int ServerPort { get; set; }
    public int ConnectionType { get; set; }
    public string TerminalType { get; set; }
    public TerminalColors Colors { get; set; }
    public int BufferSize { get; set; } = 8192;
    public int MaxLines { get; set; } = 1000;
    public bool AutoScroll { get; set; } = true;
}
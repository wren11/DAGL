namespace DarkAges.Library.Network;

/// <summary>
/// Server information
/// </summary>
public class ServerInfo
{
    public string Address { get; set; } = "";
    public int Port { get; set; }
    public string Name { get; set; } = "";
    public ServerStatus Status { get; set; }
}
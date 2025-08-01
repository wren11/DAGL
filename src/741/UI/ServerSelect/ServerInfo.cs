namespace DarkAges.Library.UI.ServerSelect;

public class ServerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Port { get; set; }
    public ServerStatus Status { get; set; }
    public int PlayerCount { get; set; }
    public int MaxPlayers { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime LastPing { get; set; }
}
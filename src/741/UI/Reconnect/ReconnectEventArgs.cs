namespace DarkAges.Library.UI.Reconnect;

public class ReconnectEventArgs : EventArgs
{
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; }
}
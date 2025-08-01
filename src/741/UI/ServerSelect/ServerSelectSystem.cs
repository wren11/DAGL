using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.ServerSelect;

public class ServerSelectSystem : ControlPane
{
    private ServerSelectDialogPane _serverSelectDialog;
    private readonly List<ServerInfo> _knownServers = [];

    public event EventHandler<ServerInfo> ServerConnectRequested;
    public event EventHandler ServerListRefreshed;

    public ServerSelectSystem()
    {
        _serverSelectDialog = new ServerSelectDialogPane();
        _serverSelectDialog.ConnectRequested += (s, server) => HandleConnectRequest(server);
        _serverSelectDialog.RefreshRequested += (s, e) => HandleRefreshRequest();
    }

    public void ShowServerSelect()
    {
        _serverSelectDialog.Show();
    }

    private void HandleConnectRequest(ServerInfo server)
    {
        ServerConnectRequested?.Invoke(this, server);
    }

    private void HandleRefreshRequest()
    {
        ServerListRefreshed?.Invoke(this, EventArgs.Empty);
    }

    public void AddKnownServer(ServerInfo server)
    {
        _knownServers.Add(server);
        _serverSelectDialog.AddServer(server);
    }

    public void UpdateServerStatus(string address, int port, ServerStatus status)
    {
        _serverSelectDialog.UpdateServerStatus(address, port, status);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _serverSelectDialog.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_serverSelectDialog.HandleEvent(e)) return true;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        _serverSelectDialog?.Dispose();
    }
}
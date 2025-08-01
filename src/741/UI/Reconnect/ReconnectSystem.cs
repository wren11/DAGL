using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Reconnect;

public class ReconnectSystem : ControlPane
{
    private ReconnectDialogPane _reconnectDialog;
    private bool _isConnected;
    private DateTime _lastConnectionCheck;

    public event EventHandler<ReconnectEventArgs> ReconnectAttempted;
    public event EventHandler ReconnectSuccessful;
    public event EventHandler ReconnectFailed;

    public ReconnectSystem()
    {
        _reconnectDialog = new ReconnectDialogPane();
        _reconnectDialog.ReconnectRequested += (s, args) => HandleReconnectRequest(args);
        _reconnectDialog.ReconnectCancelled += (s, e) => HandleReconnectCancel();
    }

    public void ShowReconnectDialog()
    {
        _reconnectDialog.ShowReconnectDialog();
    }

    private void HandleReconnectRequest(ReconnectEventArgs args)
    {
        ReconnectAttempted?.Invoke(this, args);
        AttemptReconnect(args);
    }

    private void HandleReconnectCancel()
    {
        _isConnected = false;
    }

    private async void AttemptReconnect(ReconnectEventArgs args)
    {
        try
        {
            var success = await EstablishConnection(args.ServerAddress, args.Port);
                
            if (success)
            {
                _isConnected = true;
                _lastConnectionCheck = DateTime.Now;
                ReconnectSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _isConnected = false;
                ReconnectFailed?.Invoke(this, EventArgs.Empty);
            }
        }
        catch
        {
            _isConnected = false;
            ReconnectFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task<bool> EstablishConnection(string serverAddress, int port)
    {
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(serverAddress, port);
                    
            if (client.Connected)
            {
                // Create a custom connection event
                var connectionEvent = new ConnectionEvent(serverAddress, port, ConnectionEventType.Connected);
                // For now, just log the connection since EventManager might not handle custom events
                Console.WriteLine($"Connected to {serverAddress}:{port}");
                return true;
            }
        }
        catch
        {
        }
            
        return false;
    }

    public void CheckConnection()
    {
        if (!_isConnected) return;

        if (DateTime.Now - _lastConnectionCheck > TimeSpan.FromSeconds(30))
        {
            _lastConnectionCheck = DateTime.Now;
                
            if (!IsServerReachable())
            {
                _isConnected = false;
                ShowReconnectDialog();
            }
        }
    }

    private bool IsServerReachable()
    {
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync("127.0.0.1", 2610);
            var timeoutTask = Task.Delay(3000);
                    
            var completedTask = Task.WhenAny(connectTask, timeoutTask).Result;
                    
            return completedTask == connectTask && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _reconnectDialog.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_reconnectDialog.HandleEvent(e)) return true;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        _reconnectDialog?.Dispose();
    }
}
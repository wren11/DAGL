using System;
using System.Threading.Tasks;
using DarkAges.Library.Graphics;
using System.Drawing;
using DarkAges.Library.IO;
using Size = System.Drawing.Size;

namespace DarkAges.Library.UI.Reconnect;

public class ReconnectDialogPane : ControlPane
{
    private TextButtonExControlPane _reconnectButton;
    private TextButtonExControlPane _cancelButton;
    private TextEditControlPane _serverAddressBox;
    private TextEditControlPane _portBox;
    private ImagePane _backgroundImage;
    private Rectangle _backgroundRect;
    private bool _isReconnecting;
    private int _reconnectAttempts;
    private const int MaxReconnectAttempts = 5;

    public event EventHandler<ReconnectEventArgs> ReconnectRequested;
    public event EventHandler ReconnectCancelled;

    public ReconnectDialogPane()
    {
        InitializeControls();
        LoadLayout();
    }

    private void InitializeControls()
    {
        _reconnectButton = new TextButtonExControlPane("Reconnect");
        _cancelButton = new TextButtonExControlPane("Cancel");
        _serverAddressBox = new TextEditControlPane();
        _portBox = new TextEditControlPane();

        _reconnectButton.Position = new Point(150, 250);
        _cancelButton.Position = new Point(250, 250);
        _serverAddressBox.Position = new Point(150, 150);
        _serverAddressBox.Size = new Size(200, 20);
        _portBox.Position = new Point(150, 180);
        _portBox.Size = new Size(100, 20);

        _serverAddressBox.Text = "127.0.0.1";
        _portBox.Text = "2610";

        _reconnectButton.Click += (s, e) => AttemptReconnect();
        _cancelButton.Click += (s, e) => CancelReconnect();

        AddChild(_reconnectButton);
        AddChild(_cancelButton);
        AddChild(_serverAddressBox);
        AddChild(_portBox);
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_reconnectdlg.txt");
                
            var backgroundName = layout.GetString("Background", "reconnect_bg");
            _backgroundImage = new ImagePane();
            _backgroundImage.SetImage(ImageLoader.LoadImage(backgroundName), null);

            _backgroundRect = layout.GetRect("Background", new Rectangle(200, 100, 400, 300));
                
            _reconnectButton.Bounds = layout.GetRect("Reconnect");
            _cancelButton.Bounds = layout.GetRect("Cancel");
            _serverAddressBox.Bounds = layout.GetRect("ServerAddress");
            _portBox.Bounds = layout.GetRect("Port");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading layout: {ex.Message}");
            _backgroundRect = new Rectangle(200, 100, 400, 300);
        }
    }

    private async void AttemptReconnect()
    {
        if (_isReconnecting) return;

        _isReconnecting = true;
        _reconnectAttempts = 0;
        _reconnectButton.Text = "Reconnecting...";
        _reconnectButton.Enabled = false;

        var args = new ReconnectEventArgs
        {
            ServerAddress = _serverAddressBox.Text,
            Port = int.TryParse(_portBox.Text, out var port) ? port : 2610
        };

        ReconnectRequested?.Invoke(this, args);

        await Task.Run(async () =>
        {
            while (_reconnectAttempts < MaxReconnectAttempts && _isReconnecting)
            {
                _reconnectAttempts++;
                    
                if (await TryConnect(args.ServerAddress, args.Port))
                {
                    OnReconnectSuccess();
                    return;
                }

                if (_reconnectAttempts < MaxReconnectAttempts)
                {
                    await Task.Delay(2000);
                }
            }

            OnReconnectFailed();
        });
    }

    private async Task<bool> TryConnect(string serverAddress, int port)
    {
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync(serverAddress, port);
            var timeoutTask = Task.Delay(5000);
                    
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                    
            if (completedTask == connectTask && client.Connected)
            {
                client.Close();
                return true;
            }
        }
        catch
        {
        }
            
        return false;
    }

    private void OnReconnectSuccess()
    {
        _isReconnecting = false;
        _reconnectButton.Text = "Reconnect";
        _reconnectButton.Enabled = true;
        Hide();
    }

    private void OnReconnectFailed()
    {
        _isReconnecting = false;
        _reconnectButton.Text = "Reconnect";
        _reconnectButton.Enabled = true;
            
        var graphicsDevice = GraphicsDevice.Instance;
        var font = FontManager.GetFont("default");
        graphicsDevice.DrawText("Reconnection failed. Please check your connection.", 150, 200, Color.Red, font);
    }

    private void CancelReconnect()
    {
        _isReconnecting = false;
        _reconnectButton.Text = "Reconnect";
        _reconnectButton.Enabled = true;
        ReconnectCancelled?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    public void ShowReconnectDialog()
    {
        Show();
        _isReconnecting = false;
        _reconnectAttempts = 0;
        _reconnectButton.Text = "Reconnect";
        _reconnectButton.Enabled = true;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        if (_backgroundImage != null)
        {
            _backgroundImage.Render(spriteBatch);
        }
        else
        {
            spriteBatch.DrawRectangle(_backgroundRect, System.Drawing.Color.White);
            spriteBatch.DrawRectangle(_backgroundRect, System.Drawing.Color.Black);
        }

        base.Render(spriteBatch);
    }
}
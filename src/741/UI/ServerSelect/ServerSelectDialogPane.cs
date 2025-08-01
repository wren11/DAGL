using System;
using System.Collections.Generic;
using System.Net;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;
using System.Drawing;
using Size = System.Drawing.Size;
namespace DarkAges.Library.UI.ServerSelect;

public class ServerSelectDialogPane : ControlPane
{
    private readonly List<ServerInfo> _servers = [];
    private readonly List<TextButtonExControlPane> _serverButtons = [];
    private TextButtonExControlPane _connectButton;
    private TextButtonExControlPane _refreshButton;
    private TextButtonExControlPane _cancelButton;
    private TextEditControlPane _serverAddressBox;
    private TextEditControlPane _portBox;
    private ImagePane _backgroundImage;
    private Rectangle _backgroundRect;
    private ServerInfo _selectedServer;
    private bool _isConnecting;

    public event EventHandler<ServerInfo> ServerSelected;
    public event EventHandler<ServerInfo> ConnectRequested;
    public event EventHandler RefreshRequested;

    public ServerSelectDialogPane()
    {
        InitializeControls();
        LoadLayout();
        LoadDefaultServers();
    }

    private void InitializeControls()
    {
        _connectButton = new TextButtonExControlPane("Connect");
        _refreshButton = new TextButtonExControlPane("Refresh");
        _cancelButton = new TextButtonExControlPane("Cancel");
        _serverAddressBox = new TextEditControlPane();
        _portBox = new TextEditControlPane();

        _connectButton.Position = new Point(150, 350);
        _refreshButton.Position = new Point(250, 350);
        _cancelButton.Position = new Point(350, 350);
        _serverAddressBox.Position = new Point(150, 300);
        _serverAddressBox.Size = new Size(200, 20);
        _portBox.Position = new Point(150, 320);
        _portBox.Size = new Size(100, 20);

        _serverAddressBox.Text = "127.0.0.1";
        _portBox.Text = "2610";

        _connectButton.Click += (s, e) => ConnectToServer();
        _refreshButton.Click += (s, e) => RefreshServers();
        _cancelButton.Click += (s, e) => Hide();

        AddChild(_connectButton);
        AddChild(_refreshButton);
        AddChild(_cancelButton);
        AddChild(_serverAddressBox);
        AddChild(_portBox);
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_serverselectdlg.txt");
                
            var backgroundName = layout.GetString("Background", "server_bg");
            _backgroundImage = new ImagePane();
            _backgroundImage.SetImage(ImageLoader.LoadImage(backgroundName), null);

            _backgroundRect = layout.GetRect("Background", new Rectangle(100, 100, 500, 400));

            _connectButton.Bounds = layout.GetRect("Connect");
            _refreshButton.Bounds = layout.GetRect("Refresh");
            _cancelButton.Bounds = layout.GetRect("Cancel");
            _serverAddressBox.Bounds = layout.GetRect("ServerAddress");
            _portBox.Bounds = layout.GetRect("Port");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading layout: {ex.Message}");
            _backgroundRect = new Rectangle(100, 100, 500, 400);
        }
    }

    private void LoadDefaultServers()
    {
        _servers.Add(new ServerInfo { Name = "Local Server", Address = "127.0.0.1", Port = 2610, Status = ServerStatus.Online });
        _servers.Add(new ServerInfo { Name = "Test Server", Address = "test.darkages.com", Port = 2610, Status = ServerStatus.Online });
        _servers.Add(new ServerInfo { Name = "Development Server", Address = "dev.darkages.com", Port = 2610, Status = ServerStatus.Offline });

        UpdateServerButtons();
    }

    private void UpdateServerButtons()
    {
        foreach (var button in _serverButtons)
        {
            RemoveChild(button);
        }
        _serverButtons.Clear();

        for (var i = 0; i < _servers.Count; i++)
        {
            var server = _servers[i];
            var statusText = server.Status == ServerStatus.Online ? "Online" : "Offline";
            var button = new TextButtonExControlPane($"{server.Name} ({server.Address}:{server.Port}) - {statusText}");
            button.Position = new Point(50, 100 + i * 30);
            button.Click += (s, e) => SelectServer(server);
            _serverButtons.Add(button);
            AddChild(button);
        }
    }

    private void SelectServer(ServerInfo server)
    {
        _selectedServer = server;
        _serverAddressBox.Text = server.Address;
        _portBox.Text = server.Port.ToString();
        ServerSelected?.Invoke(this, server);
    }

    private async void ConnectToServer()
    {
        if (_isConnecting) return;

        var serverAddress = _serverAddressBox.Text.Trim();
        if (!int.TryParse(_portBox.Text, out var port))
        {
            port = 2610;
        }

        var server = _selectedServer ?? new ServerInfo
        {
            Name = "Custom Server",
            Address = serverAddress,
            Port = port,
            Status = ServerStatus.Unknown
        };

        _isConnecting = true;
        _connectButton.Text = "Connecting...";
        _connectButton.IsEnabled = false;

        ConnectRequested?.Invoke(this, server);

        try
        {
            var success = await TestConnection(serverAddress, port);
            if (success)
            {
                Hide();
            }
            else
            {
                //var graphicsDevice = GraphicsDevice.Instance;
                //var font = FontManager.Instance.GetFont("default");
                //graphicsDevice.DrawText("Connection failed. Please check server address and port.", 150, 380, Color.Red, font);
            }
        }
        finally
        {
            _isConnecting = false;
            _connectButton.Text = "Connect";
            _connectButton.IsEnabled = true;
        }
    }

    private async System.Threading.Tasks.Task<bool> TestConnection(string address, int port)
    {
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync(address, port);
            var timeoutTask = System.Threading.Tasks.Task.Delay(5000);
                    
            var completedTask = await System.Threading.Tasks.Task.WhenAny(connectTask, timeoutTask);
                    
            return completedTask == connectTask && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    private async void RefreshServers()
    {
        _refreshButton.Text = "Refreshing...";
        _refreshButton.IsEnabled = false;

        RefreshRequested?.Invoke(this, EventArgs.Empty);

        foreach (var server in _servers)
        {
            server.Status = await TestConnection(server.Address, server.Port) ? ServerStatus.Online : ServerStatus.Offline;
        }

        UpdateServerButtons();

        _refreshButton.Text = "Refresh";
        _refreshButton.IsEnabled = true;
    }

    public void AddServer(ServerInfo server)
    {
        _servers.Add(server);
        UpdateServerButtons();
    }

    public void RemoveServer(ServerInfo server)
    {
        _servers.Remove(server);
        UpdateServerButtons();
    }

    public void UpdateServerStatus(string address, int port, ServerStatus status)
    {
        var server = _servers.Find(s => s.Address == address && s.Port == port);
        if (server != null)
        {
            server.Status = status;
            UpdateServerButtons();
        }
    }

    public void Show()
    {
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        if (_backgroundImage != null)
        {
            //spriteBatch.DrawImage(_backgroundImage, _backgroundRect.X, _backgroundRect.Y);
        }
        else
        {
            spriteBatch.DrawRectangle(_backgroundRect, Color.White);
            spriteBatch.DrawRectangle(_backgroundRect, Color.Black);
        }

        //spriteBatch.DrawString(font, "Server Selection", _backgroundRect.X + 200, _backgroundRect.Y + 30, Color.Black);
        //spriteBatch.DrawString(font, "Available Servers:", _backgroundRect.X + 50, _backgroundRect.Y + 70, Color.Black);
        //spriteBatch.DrawString(font, "Server Address:", _backgroundRect.X + 50, _backgroundRect.Y + 300, Color.Black);
        //spriteBatch.DrawString(font, "Port:", _backgroundRect.X + 50, _backgroundRect.Y + 320, Color.Black);
            
        base.Render(spriteBatch);
    }
}
using System;
using System.Drawing;
using System.Threading.Tasks;
using DarkAges.Library.Core;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Network;
using DarkAges.Library.Graphics;
using Size = DarkAges.Library.Graphics.Size;

namespace DarkAges.Library.UI.Connection;

/// <summary>
/// Manages network connections and provides UI for server selection and authentication
/// Based on disassembly analysis from chunks 024 and 025
/// </summary>
public class ConnectionManagerPane : Pane
{
    private const int PANE_WIDTH = 400;
    private const int PANE_HEIGHT = 300;
    private const int BUTTON_HEIGHT = 30;
    private const int INPUT_HEIGHT = 25;
    private const int MARGIN = 10;

    private NetworkManager networkManager;
    private bool isConnecting;
    private bool isAuthenticating;
    private string statusMessage;
    private DateTime lastStatusUpdate;

    // UI Components
    private TextEditControlPane serverAddressInput;
    private TextEditControlPane serverPortInput;
    private TextEditControlPane usernameInput;
    private TextEditControlPane passwordInput;
    private TextButtonExControlPane connectButton;
    private TextButtonExControlPane disconnectButton;
    private TextButtonExControlPane refreshButton;
    private TextButtonExControlPane settingsButton;
    private TextFlowPane statusPane;
    private ListPane serverListPane;
    private RadioGroupControlPane protocolGroup;

    // Connection settings
    private string currentServerAddress;
    private int currentServerPort;
    private string currentUsername;
    private string currentPassword;
    private int selectedProtocol;
    private bool useEncryption;

    public event Action ConnectionEstablished;
    public event Action ConnectionFailed;
    public event Action<string> StatusChanged;

    public ConnectionManagerPane()
    {
        InitializeComponents();
        InitializeNetworkManager();
    }

    private void InitializeComponents()
    {
        Size = new Size(PANE_WIDTH, PANE_HEIGHT);
        Position = new Point(50, 50);

        // Server address input
        serverAddressInput = new TextEditControlPane("da0.kru.com", new Rectangle(MARGIN, MARGIN, 200, INPUT_HEIGHT));
        serverAddressInput.Label = "Server Address:";

        // Server port input
        serverPortInput = new TextEditControlPane("2610", new Rectangle(MARGIN, MARGIN + INPUT_HEIGHT + 5, 100, INPUT_HEIGHT));
        serverPortInput.Label = "Port:";

        // Username input
        usernameInput = new TextEditControlPane("", new Rectangle(MARGIN, MARGIN + (INPUT_HEIGHT + 5) * 2, 150, INPUT_HEIGHT));
        usernameInput.Label = "Username:";

        // Password input
        passwordInput = new TextEditControlPane("", new Rectangle(MARGIN, MARGIN + (INPUT_HEIGHT + 5) * 3, 150, INPUT_HEIGHT));
        passwordInput.Label = "Password:";
        passwordInput.IsPassword = true;

        // Protocol selection
        protocolGroup = new RadioGroupControlPane();
        protocolGroup.AddButton(new TextButtonExControlPane("TCP"));
        protocolGroup.AddButton(new TextButtonExControlPane("UDP"));
        protocolGroup.AddButton(new TextButtonExControlPane("WebSocket"));
        protocolGroup.AddButton(new TextButtonExControlPane("HTTP"));
        protocolGroup.AddButton(new TextButtonExControlPane("HTTPS"));
        protocolGroup.AddButton(new TextButtonExControlPane("Custom"));
        protocolGroup.Position = new Point(MARGIN, MARGIN + (INPUT_HEIGHT + 5) * 4);
        protocolGroup.Size = new Size(200, 120);

        // Buttons
        connectButton = new TextButtonExControlPane("Connect");
        connectButton.Position = new Point(MARGIN, MARGIN + (INPUT_HEIGHT + 5) * 4 + 130);
        connectButton.Size = new Size(80, BUTTON_HEIGHT);
        connectButton.OnClick += OnConnectClicked;

        disconnectButton = new TextButtonExControlPane("Disconnect");
        disconnectButton.Position = new Point(MARGIN + 90, MARGIN + (INPUT_HEIGHT + 5) * 4 + 130);
        disconnectButton.Size = new Size(80, BUTTON_HEIGHT);
        disconnectButton.Enabled = false;
        disconnectButton.OnClick += OnDisconnectClicked;

        refreshButton = new TextButtonExControlPane("Refresh");
        refreshButton.Position = new Point(MARGIN + 180, MARGIN + (INPUT_HEIGHT + 5) * 4 + 130);
        refreshButton.Size = new Size(80, BUTTON_HEIGHT);
        refreshButton.OnClick += OnRefreshClicked;

        settingsButton = new TextButtonExControlPane("Settings");
        settingsButton.Position = new Point(MARGIN + 270, MARGIN + (INPUT_HEIGHT + 5) * 4 + 130);
        settingsButton.Size = new Size(80, BUTTON_HEIGHT);
        settingsButton.OnClick += OnSettingsClicked;

        // Status pane
        statusPane = new TextFlowPane(new Rectangle(MARGIN, MARGIN + (INPUT_HEIGHT + 5) * 4 + 130 + BUTTON_HEIGHT + 10, PANE_WIDTH - MARGIN * 2, 80));
        statusPane.Text = "Ready to connect";

        // Server list
        serverListPane = new ListPane(FontManager.GetFont("default") as SimpleFont);
        serverListPane.Position = new Point(220, MARGIN);
        serverListPane.Size = new Size(170, PANE_HEIGHT - MARGIN * 2);
        serverListPane.Title = "Server List";
        serverListPane.OnItemSelected += OnServerSelected;

        // Add components
        AddChild(serverAddressInput);
        AddChild(serverPortInput);
        AddChild(usernameInput);
        AddChild(passwordInput);
        AddChild(protocolGroup);
        AddChild(connectButton);
        AddChild(disconnectButton);
        AddChild(refreshButton);
        AddChild(settingsButton);
        AddChild(statusPane);
        AddChild(serverListPane);

        // Initialize default values
        currentServerAddress = "da0.kru.com";
        currentServerPort = 2610;
        currentUsername = "";
        currentPassword = "";
        selectedProtocol = 0;
        useEncryption = false;
        statusMessage = "Ready to connect";
        lastStatusUpdate = DateTime.Now;
    }

    private void OnServerSelected(object sender, int index)
    {
        if (index < 0 || index >= serverListPane.ItemCount)
            return;
        var selectedItem = serverListPane.GetItem(index);
        if (selectedItem != null)
        {
            ParseServerSelection(selectedItem);
        }
    }

    private void InitializeNetworkManager()
    {
        networkManager = new NetworkManager();
            
        // Wire up events
        networkManager.Connected += OnNetworkConnected;
        networkManager.Disconnected += OnNetworkDisconnected;
        networkManager.NetworkError += OnNetworkError;
        networkManager.StatusMessage += OnNetworkStatusMessage;
        networkManager.AuthenticationCompleted += OnAuthenticationCompleted;

        // Add default servers
        AddDefaultServers();
    }

    private void AddDefaultServers()
    {
        serverListPane.AddItem("Server 1 - 127.0.0.1:2610");
        serverListPane.AddItem("Server 2 - 192.168.1.100:2610");
        serverListPane.AddItem("Server 3 - 10.0.0.1:2610");
    }

    private async void OnConnectClicked(ControlPane sender)
    {
        if (isConnecting)
            return;

        await ConnectToServer();
    }

    private async void OnDisconnectClicked(ControlPane sender)
    {
        DisconnectFromServer();
    }

    private async void OnRefreshClicked(ControlPane sender)
    {
        await RefreshServerList();
    }

    private void OnSettingsClicked(ControlPane sender)
    {
        ShowSettingsDialog();
    }

    private async Task ConnectToServer()
    {
        try
        {
            isConnecting = true;
            UpdateStatus("Connecting...");
            UpdateButtonStates();

            // Get current values
            currentServerAddress = serverAddressInput.Text;
            currentServerPort = int.TryParse(serverPortInput.Text, out var port) ? port : 2610;
            currentUsername = usernameInput.Text;
            currentPassword = passwordInput.Text;
            selectedProtocol = protocolGroup.SelectedIndex;

            // Configure network manager
            networkManager.SetServerInfo(currentServerAddress, currentServerPort);
            networkManager.SetCredentials(currentUsername, currentPassword);
            networkManager.SetConnectionType(selectedProtocol);
            networkManager.SetEncryption(useEncryption);

            // Attempt connection
            await networkManager.ConnectAsync(currentServerAddress, currentServerPort);
                
            if (networkManager.IsConnected)
            {
                UpdateStatus("Connected! Authenticating...");
                isAuthenticating = true;
                    
                // Attempt authentication
                var authenticated = await networkManager.AuthenticateAsync();
                    
                if (authenticated)
                {
                    UpdateStatus("Connected and authenticated successfully!");
                    ConnectionEstablished?.Invoke();
                }
                else
                {
                    UpdateStatus("Authentication failed!");
                    networkManager.Disconnect();
                    ConnectionFailed?.Invoke();
                }
            }
            else
            {
                UpdateStatus("Connection failed!");
                ConnectionFailed?.Invoke();
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Connection error: {ex.Message}");
            ConnectionFailed?.Invoke();
        }
        finally
        {
            isConnecting = false;
            isAuthenticating = false;
            UpdateButtonStates();
        }
    }

    private void DisconnectFromServer()
    {
        try
        {
            networkManager.Disconnect();
            UpdateStatus("Disconnected");
            UpdateButtonStates();
        }
        catch (Exception ex)
        {
            UpdateStatus($"Disconnect error: {ex.Message}");
        }
    }

    private async Task RefreshServerList()
    {
        try
        {
            UpdateStatus("Refreshing server list...");
                
            // Clear current list
            serverListPane.Clear();
                
            // Add default servers
            AddDefaultServers();
                
            // Test server connectivity
            await TestServerConnectivity();
                
            UpdateStatus("Server list refreshed");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Refresh error: {ex.Message}");
        }
    }

    private async Task TestServerConnectivity()
    {
        // Test each server in the list
        for (var i = 0; i < serverListPane.ItemCount; i++)
        {
            var item = serverListPane.GetItem(i);
            if (item != null)
            {
                var isOnline = await TestServer(item);

                // Update item with status
                var status = isOnline ? " (Online)" : " (Offline)";
                serverListPane.SetItem(i, item + status);
            }
        }
    }

    private async Task<bool> TestServer(string serverInfo)
    {
        try
        {
            var parts = serverInfo.Split(':');
            if (parts.Length >= 2)
            {
                var address = parts[0];
                var port = int.TryParse(parts[1].Split(' ')[0], out var p) ? p : 2610;

                using var client = new System.Net.Sockets.TcpClient();
                var connectTask = client.ConnectAsync(address, port);
                var timeoutTask = Task.Delay(3000);
                        
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                return completedTask == connectTask && client.Connected;
            }
        }
        catch
        {
        }
            
        return false;
    }

    private void ParseServerSelection(string item)
    {
        try
        {
            var parts = item.Split(':');
            if (parts.Length >= 2)
            {
                var address = parts[0];
                var portPart = parts[1].Split(' ')[0];
                    
                if (int.TryParse(portPart, out var port))
                {
                    serverAddressInput.Text = address;
                    serverPortInput.Text = port.ToString();
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }
    }

    private void ShowSettingsDialog()
    {
        // Create and show settings dialog
        var settingsDialog = new ConnectionSettingsDialog();
        settingsDialog.Show();
    }

    private void UpdateStatus(string message)
    {
        statusMessage = message;
        statusPane.Text = message;
        lastStatusUpdate = DateTime.Now;
        StatusChanged?.Invoke(message);
    }

    private void UpdateButtonStates()
    {
        var connected = networkManager.IsConnected;
            
        connectButton.Enabled = !isConnecting && !connected;
        disconnectButton.Enabled = connected;
        refreshButton.Enabled = !isConnecting;
        settingsButton.Enabled = !isConnecting;
            
        serverAddressInput.Enabled = !isConnecting && !connected;
        serverPortInput.Enabled = !isConnecting && !connected;
        usernameInput.Enabled = !isConnecting && !connected;
        passwordInput.Enabled = !isConnecting && !connected;
        protocolGroup.Enabled = !isConnecting && !connected;
    }

    private void OnNetworkConnected(object sender, SocketEventArgs e)
    {
        UpdateStatus("Network connected");
        UpdateButtonStates();
    }

    private void OnNetworkDisconnected(object sender, SocketEventArgs e)
    {
        UpdateStatus("Network disconnected");
        UpdateButtonStates();
    }

    private void OnNetworkError(object sender, NetworkErrorEventArgs e)
    {
        UpdateStatus($"Network error: {e.Error.Message}");
    }

    private void OnNetworkStatusMessage(object sender, string message)
    {
        UpdateStatus(message);
    }

    private void OnAuthenticationCompleted(object sender, AuthenticationResult result)
    {
        if (result.Success)
        {
            UpdateStatus("Authentication successful");
        }
        else
        {
            UpdateStatus($"Authentication failed: {result.Error}");
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible)
            return;

        // Render background
        var graphicsDevice = GraphicsDevice.Instance;
        graphicsDevice.FillRectangle(Position.X, Position.Y, Size.Width, Size.Height, Color.FromArgb(200, 0, 0, 0));

        // Render title
        var font = FontManager.GetFont("default");
        graphicsDevice.DrawText("Connection Manager", Position.X + 10, Position.Y + 5, Color.White, font);

        // Render components
        base.Render(spriteBatch);

        // Render connection status indicator
        var statusColor = networkManager.IsConnected ? Color.Green : Color.Red;
        //graphicsDevice.FillCircle(Position.X + Size.Width - 20, Position.Y + 10, 5, statusColor);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible)
            return false;

        return base.HandleEvent(e);
    }

    public void SetCredentials(string username, string password)
    {
        usernameInput.Text = username;
        passwordInput.Text = password;
    }

    public void SetServer(string address, int port)
    {
        serverAddressInput.Text = address;
        serverPortInput.Text = port.ToString();
    }

    public NetworkManager GetNetworkManager()
    {
        return networkManager;
    }

    public override void Dispose()
    {
        networkManager?.Dispose();
        base.Dispose();
    }
}
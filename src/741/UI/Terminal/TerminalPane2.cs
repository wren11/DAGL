using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DarkAges.Library.Core;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using DarkAges.Library.Network;
using System.Drawing;
using Size = DarkAges.Library.Graphics.Size;

namespace DarkAges.Library.UI.Terminal;

/// <summary>
/// Terminal pane for handling console-like communication with network protocols
/// Based on disassembly analysis from chunk 026
/// </summary>
public class TerminalPane2 : ControlPane
{
    private const int TERMINAL_WIDTH = 640;
    private const int TERMINAL_HEIGHT = 480;
    private const int BUFFER_SIZE = 0x7FFF;
    private const int MAX_LINE_LENGTH = 255;
    private const int CONNECTION_TIMEOUT = 30000;
    private const int RETRY_DELAY = 3000;

    // Terminal state
    private int terminalState;
    private bool isConnected;
    private bool isWaitingReply;
    private bool isMouseOverExit;
    private DateTime lastConnectionAttempt;
    private int connectionRetries;

    // Terminal buffer and display
    private StringBuilder terminalBuffer;
    private string[] displayLines;
    private int currentLine;
    private int maxLines;
    private int scrollPosition;
    private bool autoScroll;

    // Network communication
    private NetworkManager networkManager;
    private byte[] receiveBuffer;
    private int receiveBufferPos;

    // UI components
    private TextFlowPane terminalDisplay;
    private TextButtonExControlPane exitButton;
    private TextButtonExControlPane connectButton;
    private TextButtonExControlPane disconnectButton;
    private int cursorPosition;

    // Terminal colors and formatting
    private System.Drawing.Color backgroundColor;
    private System.Drawing.Color textColor;
    private System.Drawing.Color cursorColor;
    private bool boldText;
    private bool italicText;
    private bool underlineText;

    // ANSI escape sequence handling
    private StringBuilder escapeSequence;
    private bool inEscapeSequence;
    private int escapeParameter;

    // Connection settings
    private string serverAddress;
    private int serverPort;
    private string username;
    private string password;
    private int connectionType;

    public event Action Connected;
    public event Action Disconnected;
    public event Action<string> DataReceived;
    public event Action<string> DataSent;

    public TerminalPane2()
    {
        InitializeTerminal();
    }

    private void InitializeTerminal()
    {
        Size = new Size(TERMINAL_WIDTH, TERMINAL_HEIGHT);
        Position = new Point(50, 50);

        // Initialize terminal state
        terminalState = 0;
        isConnected = false;
        isWaitingReply = false;
        isMouseOverExit = false;
        lastConnectionAttempt = DateTime.MinValue;
        connectionRetries = 0;

        // Initialize buffers
        terminalBuffer = new StringBuilder();
        maxLines = 1000;
        displayLines = new string[maxLines];
        currentLine = 0;
        scrollPosition = 0;
        autoScroll = true;

        // Initialize network
        networkManager = new NetworkManager();
        receiveBuffer = new byte[BUFFER_SIZE];
        receiveBufferPos = 0;

        // Wire up network events
        networkManager.Connected += OnNetworkConnected;
        networkManager.Disconnected += OnNetworkDisconnected;
        networkManager.DataReceived += OnNetworkDataReceived;
        networkManager.StatusMessage += OnNetworkStatusMessage;

        // Initialize colors
        backgroundColor = System.Drawing.Color.Black;
        textColor = System.Drawing.Color.White;
        cursorColor = System.Drawing.Color.Green;
        boldText = false;
        italicText = false;
        underlineText = false;

        // Initialize ANSI handling
        escapeSequence = new StringBuilder();
        inEscapeSequence = false;
        escapeParameter = 0;

        // Initialize connection settings
        serverAddress = "da0.kru.com";
        serverPort = 2610;
        username = "";
        password = "";
        connectionType = 0;

        // Create UI components
        CreateUIComponents();

        // Load terminal configuration
        LoadTerminalConfiguration();
    }

    private void CreateUIComponents()
    {
        // Terminal display area
        terminalDisplay = new TextFlowPane(new Rectangle(10, 10, TERMINAL_WIDTH - 20, TERMINAL_HEIGHT - 80));
        terminalDisplay.Text = "Terminal Ready\n";

        // Exit button
        exitButton = new TextButtonExControlPane("Exit");
        exitButton.Position = new Point(TERMINAL_WIDTH - 80, TERMINAL_HEIGHT - 60);
        exitButton.Size = new Size(70, 25);
        exitButton.Click += OnExitClicked;

        // Connect button
        connectButton = new TextButtonExControlPane("Connect");
        connectButton.Position = new Point(10, TERMINAL_HEIGHT - 60);
        connectButton.Size = new Size(70, 25);
        connectButton.Click += OnConnectClicked;

        // Disconnect button
        disconnectButton = new TextButtonExControlPane("Disconnect");
        disconnectButton.Position = new Point(90, TERMINAL_HEIGHT - 60);
        disconnectButton.Size = new Size(70, 25);
        disconnectButton.Enabled = false;
        disconnectButton.Click += OnDisconnectClicked;

        // Add components
        AddChild(terminalDisplay);
        AddChild(exitButton);
        AddChild(connectButton);
        AddChild(disconnectButton);
    }

    private void LoadTerminalConfiguration()
    {
        try
        {
            // Load terminal configuration from file
            var configFile = "lterm2.txt";
            if (System.IO.File.Exists(configFile))
            {
                string[] lines = System.IO.File.ReadAllLines(configFile);
                foreach (var line in lines)
                {
                    ParseConfigurationLine(line);
                }
            }
        }
        catch
        {
            // Use defaults if configuration cannot be loaded
            WriteToTerminal("Configuration loaded with defaults\n");
        }
    }

    private void ParseConfigurationLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        string[] parts = line.Split('=');
        if (parts.Length != 2)
            return;

        var key = parts[0].Trim();
        var value = parts[1].Trim();

        switch (key.ToLower())
        {
        case "server":
            serverAddress = value;
            break;
        case "port":
            if (int.TryParse(value, out var port))
                serverPort = port;
            break;
        case "username":
            username = value;
            break;
        case "password":
            password = value;
            break;
        case "connectiontype":
            if (int.TryParse(value, out var type))
                connectionType = type;
            break;
        }
    }

    public async void Connect()
    {
        if (isConnected)
            return;

        try
        {
            WriteToTerminal($"Connecting to {serverAddress}:{serverPort}...\n");
                
            // Configure network manager
            networkManager.SetCredentials(username, password);
            networkManager.SetConnectionType(connectionType);

            // Attempt connection
            var connected = await networkManager.ConnectAsync(serverAddress, serverPort);
                
            if (connected)
            {
                WriteToTerminal("Connected! Waiting for server reply...\n");
                isWaitingReply = true;
                    
                // Attempt authentication
                var authenticated = await networkManager.AuthenticateAsync();
                    
                if (authenticated)
                {
                    WriteToTerminal("Authentication successful!\n");
                    isConnected = true;
                    Connected?.Invoke();
                }
                else
                {
                    WriteToTerminal("Authentication failed!\n");
                    networkManager.Disconnect();
                }
            }
            else
            {
                WriteToTerminal("Connection failed!\n");
            }
        }
        catch (Exception ex)
        {
            WriteToTerminal($"Connection error: {ex.Message}\n");
        }
    }

    public void Disconnect()
    {
        if (!isConnected)
            return;

        try
        {
            networkManager.Disconnect();
            isConnected = false;
            isWaitingReply = false;
            WriteToTerminal("Disconnected\n");
            Disconnected?.Invoke();
        }
        catch (Exception ex)
        {
            WriteToTerminal($"Disconnect error: {ex.Message}\n");
        }
    }

    public void SendData(string data)
    {
        if (!isConnected)
            return;

        try
        {
            var dataBytes = Encoding.ASCII.GetBytes(data);
            networkManager.SendAsync(dataBytes);
            DataSent?.Invoke(data);
        }
        catch (Exception ex)
        {
            WriteToTerminal($"Send error: {ex.Message}\n");
        }
    }

    public void SendData(byte[] data)
    {
        if (!isConnected)
            return;

        try
        {
            networkManager.SendAsync(data);
            DataSent?.Invoke(Encoding.ASCII.GetString(data));
        }
        catch (Exception ex)
        {
            WriteToTerminal($"Send error: {ex.Message}\n");
        }
    }

    private void WriteToTerminal(string text)
    {
        lock (terminalBuffer)
        {
            terminalBuffer.Append(text);
                
            // Split into lines
            string[] lines = text.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    displayLines[currentLine] = line;
                    currentLine = (currentLine + 1) % maxLines;
                }
            }

            // Update display
            UpdateTerminalDisplay();
        }
    }

    private void UpdateTerminalDisplay()
    {
        if (terminalDisplay != null)
        {
            // Build display text from recent lines
            var displayText = new StringBuilder();
            var startLine = autoScroll ? Math.Max(0, currentLine - 50) : scrollPosition;
            var endLine = autoScroll ? currentLine : Math.Min(startLine + 50, maxLines);

            for (var i = startLine; i < endLine; i++)
            {
                if (displayLines[i] != null)
                {
                    displayText.AppendLine(displayLines[i]);
                }
            }

            terminalDisplay.Text = displayText.ToString();
        }
    }

    private void ProcessReceivedData(byte[] data)
    {
        foreach (var b in data)
        {
            ProcessByte(b);
        }
    }

    private void ProcessByte(byte b)
    {
        if (inEscapeSequence)
        {
            ProcessEscapeSequence(b);
        }
        else
        {
            ProcessNormalByte(b);
        }
    }

    private void ProcessNormalByte(byte b)
    {
        switch (b)
        {
        case 0x00: // Null
            break;
        case 0x07: // Bell
            PlayBell();
            break;
        case 0x08: // Backspace
            HandleBackspace();
            break;
        case 0x09: // Tab
            HandleTab();
            break;
        case 0x0A: // Line feed
            HandleLineFeed();
            break;
        case 0x0D: // Carriage return
            HandleCarriageReturn();
            break;
        case 0x1B: // Escape
            StartEscapeSequence();
            break;
        case 0xFF: // Special command
            HandleSpecialCommand();
            break;
        default:
            if (b >= 0x20 && b <= 0x7E) // Printable ASCII
            {
                HandlePrintableCharacter((char)b);
            }
            break;
        }
    }

    private void ProcessEscapeSequence(byte b)
    {
        escapeSequence.Append((char)b);

        switch (terminalState)
        {
        case 0: // Normal state
            if (b == '[')
            {
                terminalState = 1;
            }
            else if (b == '(' || b == ')' || b == '*' || b == '+')
            {
                terminalState = 4;
            }
            else if (b == 'S')
            {
                HandleServerMode();
                terminalState = 7;
            }
            else if (b == 'C')
            {
                HandleClientMode();
                terminalState = 7;
            }
            else
            {
                terminalState = 0;
            }
            break;

        case 1: // ESC [ received
            if (b == '0' || b == 'A' || b == 'B')
            {
                terminalState = 0;
            }
            else
            {
                terminalState = 0;
            }
            break;

        case 4: // ESC ( or ) or * or + received
            if (b == '0' || b == 'A' || b == 'B')
            {
                terminalState = 0;
            }
            else
            {
                terminalState = 0;
            }
            break;

        case 5: // ESC FF received
            if (b == 0xFD)
            {
                terminalState = 6;
            }
            else
            {
                terminalState = 0;
            }
            break;

        case 6: // ESC FF FD received
            HandleTerminalNegotiation(b);
            terminalState = 0;
            break;

        case 7: // Server/Client mode
            HandleModeComplete();
            terminalState = 0;
            break;
        }

        if (terminalState == 0)
        {
            inEscapeSequence = false;
            escapeSequence.Clear();
        }
    }

    private void StartEscapeSequence()
    {
        inEscapeSequence = true;
        escapeSequence.Clear();
        escapeParameter = 0;
    }

    private void HandlePrintableCharacter(char c)
    {
        WriteToTerminal(c.ToString());
    }

    private void HandleBackspace()
    {
        // Remove last character from current line
        if (terminalBuffer.Length > 0)
        {
            terminalBuffer.Length--;
            UpdateTerminalDisplay();
        }
    }

    private void HandleTab()
    {
        WriteToTerminal("    "); // 4 spaces
    }

    private void HandleLineFeed()
    {
        WriteToTerminal("\n");
    }

    private void HandleCarriageReturn()
    {
        // Move cursor to beginning of line
        if (terminalDisplay != null)
        {
            var currentText = terminalDisplay.Text;
            var lines = currentText.Split('\n');
            if (lines.Length > 0)
            {
                var lastLineIndex = currentText.LastIndexOf('\n');
                if (lastLineIndex >= 0)
                {
                    // Move cursor to start of current line
                    cursorPosition = lastLineIndex + 1;
                }
                else
                {
                    cursorPosition = 0;
                }
            }
        }
    }

    private void HandleSpecialCommand()
    {
        terminalState = 5;
    }

    private void HandleServerMode()
    {
        WriteToTerminal("Server Connected Mode\n");
        // Handle server mode specific logic
    }

    private void HandleClientMode()
    {
        WriteToTerminal("Connected Server Mode\n");
        // Handle client mode specific logic
    }

    private void HandleTerminalNegotiation(byte b)
    {
        if (b == 24) // Terminal type negotiation
        {
            var response = "\xFF\xFB\x18\xFF\xFA\x18";
            SendData(response);
            SendData("\xFF\xF0dumb");
        }
        else
        {
            var response = $"\xFF\xFC{(char)b}";
            SendData(response);
        }
    }

    private void HandleModeComplete()
    {
        // Handle completion of server/client mode setup
        WriteToTerminal("Mode setup complete\n");
    }

    private void PlayBell()
    {
        // Play system bell sound
        System.Console.Beep();
    }

    private void OnNetworkConnected(object sender, SocketEventArgs e)
    {
        WriteToTerminal("Network connected\n");
        UpdateButtonStates();
    }

    private void OnNetworkDisconnected(object sender, SocketEventArgs e)
    {
        WriteToTerminal("Network disconnected\n");
        isConnected = false;
        isWaitingReply = false;
        UpdateButtonStates();
    }

    private void OnNetworkDataReceived(object sender, SocketDataEventArgs e)
    {
        ProcessReceivedData(e.Data);
        DataReceived?.Invoke(Encoding.ASCII.GetString(e.Data));
    }

    private void OnNetworkStatusMessage(object sender, string message)
    {
        WriteToTerminal($"{message}\n");
    }

    private void OnConnectClicked(object sender, EventArgs e)
    {
        Connect();
    }

    private void OnDisconnectClicked(object sender, EventArgs e)
    {
        Disconnect();
    }

    private void OnExitClicked(object sender, EventArgs e)
    {
        Disconnect();
        // Close terminal
        Dispose();
    }

    private void UpdateButtonStates()
    {
        connectButton.Enabled = !isConnected;
        disconnectButton.Enabled = isConnected;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible)
            return;

        // Render background
        spriteBatch.FillRectangle(new Rectangle(Position.X, Position.Y, Size.Width, Size.Height), backgroundColor);

        // Render border
        spriteBatch.DrawRectangle(new Rectangle(Position.X, Position.Y, Size.Width, Size.Height), System.Drawing.Color.Gray);

        // Render title
        //var font = FontManager.Instance.GetFont("default");
        //spriteBatch.DrawString(font, "Terminal", Position.X + 10, Position.Y + 5, System.Drawing.Color.White);

        // Render connection status
        var statusColor = isConnected ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        var statusText = isConnected ? "Connected" : "Disconnected";
        //spriteBatch.DrawString(font, statusText, Position.X + Size.Width - 100, Position.Y + 5, statusColor);

        // Render components
        base.Render(spriteBatch);

        // Render cursor if connected
        if (isConnected)
        {
            var cursorX = Position.X + 15 + (cursorPosition % 80) * 8;
            var cursorY = Position.Y + 25 + (cursorPosition / 80) * 16;
            spriteBatch.FillRectangle(new Rectangle(cursorX, cursorY, 8, 16), cursorColor);
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible)
            return false;

        // Handle mouse events for exit button
        if (e is MouseEvent mouseEvent)
        {
            if (mouseEvent.Type == EventType.MouseMove)
            {
                var wasOverExit = isMouseOverExit;
                isMouseOverExit = IsPointInExitButton(mouseEvent.X, mouseEvent.Y);
                    
                if (wasOverExit != isMouseOverExit)
                {
                    // Update button appearance
                }
            }
        }

        return base.HandleEvent(e);
    }

    private bool IsPointInExitButton(int x, int y)
    {
        return x >= exitButton.Position.X && x <= exitButton.Position.X + exitButton.Size.Width &&
                y >= exitButton.Position.Y && y <= exitButton.Position.Y + exitButton.Size.Height;
    }

    public void SetConnectionInfo(string address, int port, string user, string pass, int type)
    {
        serverAddress = address;
        serverPort = port;
        username = user;
        password = pass;
        connectionType = type;
    }

    public void ClearTerminal()
    {
        lock (terminalBuffer)
        {
            terminalBuffer.Clear();
            Array.Clear(displayLines, 0, displayLines.Length);
            currentLine = 0;
            scrollPosition = 0;
            UpdateTerminalDisplay();
        }
    }

    public void SetTerminalColors(System.Drawing.Color background, System.Drawing.Color text, System.Drawing.Color cursor)
    {
        backgroundColor = background;
        textColor = text;
        cursorColor = cursor;
    }

    public override void Dispose()
    {
        networkManager?.Dispose();
        base.Dispose();
    }
}
using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using DarkAges.Library.Core;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using DarkAges.Library.Network;

namespace DarkAges.Library.UI.Terminal;

/// <summary>
/// Manages terminal sessions and handles terminal protocols
/// Based on disassembly analysis from chunk 026
/// </summary>
public class TerminalManager : IDisposable
{
    private const int MAX_TERMINAL_SESSIONS = 10;
    private const int TERMINAL_TIMEOUT = 30000;
    private const int CONNECTION_RETRY_DELAY = 3000;
    private const int MAX_CONNECTION_RETRIES = 5;

    private List<TerminalSession> activeSessions;
    private Dictionary<string, TerminalConfiguration> terminalConfigs;
    private NetworkManager networkManager;
    private bool isDisposed;
    private int nextSessionId;

    // Terminal state management
    private bool isTerminalMode;
    private int currentTerminalState;
    private StringBuilder terminalBuffer;
    private byte[] receiveBuffer;
    private int receiveBufferPos;

    // Events
    public event Action<TerminalSession> SessionCreated;
    public event Action<TerminalSession> SessionClosed;
    public event Action<TerminalSession, string> DataReceived;
    public event Action<TerminalSession, string> DataSent;
    public event Action<string> StatusMessage;

    public TerminalManager()
    {
        InitializeManager();
    }

    private void InitializeManager()
    {
        activeSessions = [];
        terminalConfigs = new Dictionary<string, TerminalConfiguration>();
        networkManager = new NetworkManager();
        isDisposed = false;
        nextSessionId = 1;

        // Initialize terminal state
        isTerminalMode = false;
        currentTerminalState = 0;
        terminalBuffer = new StringBuilder();
        receiveBuffer = new byte[8192];
        receiveBufferPos = 0;

        // Wire up network events
        networkManager.Connected += OnNetworkConnected;
        networkManager.Disconnected += OnNetworkDisconnected;
        networkManager.DataReceived += OnNetworkDataReceived;
        networkManager.StatusMessage += OnNetworkStatusMessage;

        // Load default terminal configurations
        LoadDefaultConfigurations();
    }

    private void LoadDefaultConfigurations()
    {
        // Add default terminal configurations
        terminalConfigs["default"] = new TerminalConfiguration
        {
            Name = "Default Terminal",
            ServerAddress = "da0.kru.com",
            ServerPort = 2610,
            ConnectionType = 0,
            TerminalType = "dumb",
            Colors = new TerminalColors
            {
                Background = System.Drawing.Color.Black,
                Text = System.Drawing.Color.White,
                Cursor = System.Drawing.Color.Green
            }
        };

        terminalConfigs["legend"] = new TerminalConfiguration
        {
            Name = "Legend Terminal",
            ServerAddress = "da0.kru.com",
            ServerPort = 2610,
            ConnectionType = 2,
            TerminalType = "legend",
            Colors = new TerminalColors
            {
                Background = System.Drawing.Color.DarkBlue,
                Text = System.Drawing.Color.Yellow,
                Cursor = System.Drawing.Color.White
            }
        };

        terminalConfigs["encrypted"] = new TerminalConfiguration
        {
            Name = "Encrypted Terminal",
            ServerAddress = "da0.kru.com",
            ServerPort = 2610,
            ConnectionType = 3,
            TerminalType = "encrypted",
            Colors = new TerminalColors
            {
                Background = System.Drawing.Color.DarkGreen,
                Text = System.Drawing.Color.LightGreen,
                Cursor = System.Drawing.Color.White
            }
        };
    }

    public TerminalSession CreateSession(string configName = "default")
    {
        if (activeSessions.Count >= MAX_TERMINAL_SESSIONS)
        {
            throw new InvalidOperationException("Maximum number of terminal sessions reached");
        }

        if (!terminalConfigs.ContainsKey(configName))
        {
            configName = "default";
        }

        var config = terminalConfigs[configName];
        var session = new TerminalSession(nextSessionId++, config);
            
        activeSessions.Add(session);
            
        // Wire up session events
        session.DataReceived += (data) => DataReceived?.Invoke(session, data);
        session.DataSent += (data) => DataSent?.Invoke(session, data);
        session.Closed += () => CloseSession(session.Id);

        SessionCreated?.Invoke(session);
        StatusMessage?.Invoke($"Terminal session {session.Id} created");

        return session;
    }

    public void CloseSession(int sessionId)
    {
        var session = activeSessions.Find(s => s.Id == sessionId);
        if (session != null)
        {
            session.Close();
            activeSessions.Remove(session);
            SessionClosed?.Invoke(session);
            StatusMessage?.Invoke($"Terminal session {sessionId} closed");
        }
    }

    public void CloseAllSessions()
    {
        foreach (var session in activeSessions.ToArray())
        {
            CloseSession(session.Id);
        }
    }

    public TerminalSession GetSession(int sessionId)
    {
        return activeSessions.Find(s => s.Id == sessionId);
    }

    public List<TerminalSession> GetAllSessions()
    {
        return [..activeSessions];
    }

    public async Task ConnectSession(int sessionId, string username = "", string password = "")
    {
        var session = GetSession(sessionId);
        if (session == null)
            return;

        try
        {
            StatusMessage?.Invoke($"Connecting session {sessionId} to {session.Config.ServerAddress}:{session.Config.ServerPort}");

            // Configure network manager for this session
            networkManager.SetServerInfo(session.Config.ServerAddress, session.Config.ServerPort);
            networkManager.SetCredentials(username, password);
            networkManager.SetConnectionType(session.Config.ConnectionType);

            // Attempt connection
            var connected = await networkManager.ConnectAsync(session.Config.ServerAddress, session.Config.ServerPort);
                
            if (connected)
            {
                session.SetConnected(true);
                StatusMessage?.Invoke($"Session {sessionId} connected successfully");
                    
                // Send terminal type negotiation if needed
                if (session.Config.ConnectionType == 0)
                {
                    await SendTerminalNegotiation(session);
                }
            }
            else
            {
                StatusMessage?.Invoke($"Session {sessionId} connection failed");
            }
        }
        catch (Exception ex)
        {
            StatusMessage?.Invoke($"Session {sessionId} connection error: {ex.Message}");
        }
    }

    public void DisconnectSession(int sessionId)
    {
        var session = GetSession(sessionId);
        if (session != null)
        {
            networkManager.Disconnect();
            session.SetConnected(false);
            StatusMessage?.Invoke($"Session {sessionId} disconnected");
        }
    }

    public async Task SendDataToSession(int sessionId, string data)
    {
        var session = GetSession(sessionId);
        if (session != null && session.IsConnected)
        {
            try
            {
                var dataBytes = Encoding.ASCII.GetBytes(data);
                await networkManager.SendAsync(dataBytes);
                session.WriteToTerminal($"Sent: {data}");
                DataSent?.Invoke(session, data);
            }
            catch (Exception ex)
            {
                session.WriteToTerminal($"Send error: {ex.Message}");
            }
        }
    }

    public async Task SendDataToSession(int sessionId, byte[] data)
    {
        var session = GetSession(sessionId);
        if (session != null && session.IsConnected)
        {
            try
            {
                await networkManager.SendAsync(data);
                session.WriteToTerminal($"Sent: {data.Length} bytes");
                DataSent?.Invoke(session, Encoding.ASCII.GetString(data));
            }
            catch (Exception ex)
            {
                session.WriteToTerminal($"Send error: {ex.Message}");
            }
        }
    }

    private async Task SendTerminalNegotiation(TerminalSession session)
    {
        try
        {
            // Send terminal type negotiation
            var negotiation = $"\xFF\xFB\x18\xFF\xFA\x18{session.Config.TerminalType}\xFF\xF0";
            var data = Encoding.ASCII.GetBytes(negotiation);
            await networkManager.SendAsync(data);
                
            session.WriteToTerminal($"Terminal type: {session.Config.TerminalType}");
        }
        catch (Exception ex)
        {
            session.WriteToTerminal($"Terminal negotiation error: {ex.Message}");
        }
    }

    private void OnNetworkConnected(object? sender, SocketEventArgs e)
    {
        StatusMessage?.Invoke("Network connected");
    }

    private void OnNetworkDisconnected(object? sender, SocketEventArgs e)
    {
        StatusMessage?.Invoke("Network disconnected");
            
        // Mark all sessions as disconnected
        foreach (var session in activeSessions)
        {
            session.SetConnected(false);
        }
    }

    private void OnNetworkDataReceived(object? sender, SocketDataEventArgs e)
    {
        // Process received data and distribute to appropriate sessions
        ProcessReceivedData(e.Data);
    }

    private void OnNetworkStatusMessage(object? sender, string message)
    {
        StatusMessage?.Invoke(message);
    }

    private void ProcessReceivedData(byte[] data)
    {
        foreach (var b in data)
        {
            ProcessByte(b);
        }

        // Distribute processed data to active sessions
        var processedData = terminalBuffer.ToString();
        if (!string.IsNullOrEmpty(processedData))
        {
            foreach (var session in activeSessions)
            {
                if (session.IsConnected)
                {
                    session.WriteToTerminal(processedData);
                }
            }
            terminalBuffer.Clear();
        }
    }

    private void ProcessByte(byte b)
    {
        // Process individual bytes based on terminal state
        switch (currentTerminalState)
        {
        case 0: // Normal state
            ProcessNormalByte(b);
            break;
        case 1: // Escape sequence
            ProcessEscapeSequence(b);
            break;
        case 2: // Terminal negotiation
            ProcessTerminalNegotiation(b);
            break;
        default:
            currentTerminalState = 0;
            break;
        }
    }

    private void ProcessNormalByte(byte b)
    {
        if (b == 0x1B) // ESC
        {
            currentTerminalState = 1;
            return;
        }

        if (b == 0x08) // Backspace
        {
            HandleBackspace();
            return;
        }

        if (b == 0x0A) // Line Feed
        {
            HandleLineFeed();
            return;
        }

        if (b == 0x0D) // Carriage Return
        {
            HandleCarriageReturn();
            return;
        }

        if (b == 0x07) // Bell
        {
            PlayBell();
            return;
        }

        // Add character to terminal buffer
        terminalBuffer.Append((char)b);
    }

    private void ProcessEscapeSequence(byte b)
    {
        switch (b)
        {
        case (byte)'[':
            currentTerminalState = 2;
            break;
        case (byte)'S':
            HandleServerMode();
            currentTerminalState = 0;
            break;
        case (byte)'C':
            HandleClientMode();
            currentTerminalState = 0;
            break;
        default:
            currentTerminalState = 0;
            break;
        }
    }

    private void ProcessTerminalNegotiation(byte b)
    {
        // Handle terminal negotiation
        currentTerminalState = 0;
    }

    private void HandleBackspace()
    {
        if (terminalBuffer.Length > 0)
        {
            terminalBuffer.Length--;
        }
    }

    private void HandleLineFeed()
    {
        terminalBuffer.Append('\n');
    }

    private void HandleCarriageReturn()
    {
        // Move cursor to beginning of line
    }

    private void HandleServerMode()
    {
        StatusMessage?.Invoke("Server mode activated");
    }

    private void HandleClientMode()
    {
        StatusMessage?.Invoke("Client mode activated");
    }

    private void PlayBell()
    {
        System.Console.Beep();
    }

    public void AddTerminalConfiguration(string name, TerminalConfiguration config)
    {
        terminalConfigs[name] = config;
    }

    public TerminalConfiguration GetTerminalConfiguration(string name)
    {
        return terminalConfigs.ContainsKey(name) ? terminalConfigs[name] : null;
    }

    public List<string> GetAvailableConfigurations()
    {
        return [..terminalConfigs.Keys];
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            CloseAllSessions();
            networkManager?.Dispose();
            isDisposed = true;
        }
    }
}
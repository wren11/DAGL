using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for Dictionary
using System.Security.Cryptography; // Added for MD5

namespace DarkAges.Library.Network;

public class NetworkManager : IDisposable
{
    private static NetworkManager _instance;
    public static NetworkManager Instance => _instance ??= new NetworkManager();

    private NetworkConnection _connection;
    private NetworkErrorHandler _errorHandler;
    private NetworkStatistics _statistics;
    private NetworkEncryption _encryption;
    private bool _isDisposed;
    private int _retryCount;
    private CancellationTokenSource _cancellationTokenSource;
    private string _username;
    private string _password;
    private bool _useEncryption;

    public event EventHandler<NetworkErrorEventArgs> NetworkError;
    public event EventHandler<SocketDataEventArgs> DataReceived;
    public event EventHandler<SocketDataEventArgs> DataSent;
    public event EventHandler<SocketEventArgs> Connected;
    public event EventHandler<SocketEventArgs> Disconnected;
    public event EventHandler<string> StatusMessage;
    public event EventHandler<AuthenticationResult> AuthenticationCompleted;


    public NetworkManager()
    {
        _connection = new NetworkConnection();
        _errorHandler = new NetworkErrorHandler();
        _statistics = new NetworkStatistics();
        _encryption = new NetworkEncryption();
        _retryCount = 3;
        _cancellationTokenSource = new CancellationTokenSource();

        _connection.DataReceived += OnDataReceived;
        _connection.DataSent += OnDataSent;
        _connection.Connected += OnConnected;
        _connection.Disconnected += OnDisconnected;
        _connection.Error += OnError;
    }

    public bool IsConnected => _connection.IsConnected;

    public void SetServerInfo(string address, int port)
    {
        // This is a placeholder. In a real implementation, this would
        // likely store the server info for later use.
    }

    public void SetCredentials(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public void SetConnectionType(int protocol)
    {
        // This is a placeholder. In a real implementation, this would
        // likely set the connection type for later use.
    }

    public void SetEncryption(bool useEncryption)
    {
        _useEncryption = useEncryption;
    }

    public async Task<bool> AuthenticateAsync()
    {
        if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
            throw new InvalidOperationException("Username and password must be set.");

        var loginRequest = new LoginRequestMessage
        {
            Username = _username,
            HashedPassword = DarkAges.Library.Common.MD5.ComputeHash(_password)
        };

        var packet = new NetworkPacket
        {
            Type = PacketType.LoginRequest,
            Data = new Dictionary<string, object>
            {
                { "Username", loginRequest.Username },
                { "HashedPassword", loginRequest.HashedPassword }
            }
        };

        await SendAsync(packet.Serialize());

        // This is a placeholder for handling the server's response.
        // In a real implementation, we would wait for a LoginResponse.
        await Task.Delay(1000);
        var result = new AuthenticationResult { Success = true };
        AuthenticationCompleted?.Invoke(this, result);
        return true;
    }

    public async Task<bool> ConnectAsync(string address, int port)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkManager));

        try
        {
            await _connection.ConnectAsync(address, port);
            return IsConnected;
        }
        catch (Exception ex)
        {
            HandleError(new NetworkError(NetworkErrorCode.ConnectionFailed, ex));
            return false;
        }
    }

    public void Disconnect()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkManager));

        try
        {
            _connection.Disconnect();
        }
        catch (Exception ex)
        {
            HandleError(new NetworkError(NetworkErrorCode.DisconnectionFailed, ex));
        }
    }

    public async Task SendAsync(byte[] data)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkManager));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        try
        {
            var dataToSend = _useEncryption ? _encryption.Encrypt(data) : data;
            await _connection.SendAsync(dataToSend);
            _statistics.BytesSent += data.Length;
        }
        catch (Exception ex)
        {
            HandleError(new NetworkError(NetworkErrorCode.SendFailed, ex));
        }
    }

    public void HandleError(NetworkError error)
    {
        if (_isDisposed)
            return;

        var result = _errorHandler.HandleError(error);
        if (result.Severity == NetworkErrorSeverity.Fatal)
        {
            Disconnect();
        }

        NetworkError?.Invoke(this, new NetworkErrorEventArgs(error));
    }

    public void OnDataReceived(object sender, SocketDataEventArgs e)
    {
        if (_isDisposed)
            return;

        try
        {
            var receivedData = _useEncryption ? _encryption.Decrypt(e.Data) : e.Data;
            _statistics.BytesReceived += receivedData.Length;
            DataReceived?.Invoke(this, new SocketDataEventArgs(receivedData));
        }
        catch (Exception ex)
        {
            HandleError(new NetworkError(NetworkErrorCode.DecryptionFailed, ex));
        }
    }

    private void OnDataSent(object sender, SocketDataEventArgs e)
    {
        if (_isDisposed)
            return;

        DataSent?.Invoke(this, e);
    }

    private void OnConnected(object sender, SocketEventArgs e)
    {
        if (_isDisposed)
            return;

        Connected?.Invoke(this, e);
    }

    private void OnDisconnected(object sender, SocketEventArgs e)
    {
        if (_isDisposed)
            return;

        Disconnected?.Invoke(this, e);
    }

    private void OnError(object sender, NetworkErrorEventArgs e)
    {
        if (_isDisposed)
            return;

        HandleError(e.Error);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _cancellationTokenSource.Cancel();
        _connection.Dispose();
        _cancellationTokenSource.Dispose();
    }
}
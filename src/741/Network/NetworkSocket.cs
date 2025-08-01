using System.Net;
using System.Net.Sockets;

namespace DarkAges.Library.Network;

public class NetworkSocket : IDisposable
{
    private readonly TcpClient _client;
    private NetworkStream _stream;
    private bool _isConnected;
    private bool _isDisposed;
    private readonly object _lockObject = new();

    public event EventHandler<SocketEventArgs> Connected;
    public event EventHandler<SocketEventArgs> Disconnected;
    public event EventHandler<SocketDataEventArgs> DataReceived;
    public event EventHandler<SocketErrorEventArgs> ErrorOccurred;

    public NetworkSocket()
    {
        _client = new TcpClient();
    }

    public NetworkSocket(TcpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _stream = client.GetStream();
        _isConnected = client.Connected;
    }

    public bool IsConnected
    {
        get
        {
            lock (_lockObject)
            {
                return _isConnected && _client?.Connected == true;
            }
        }
    }

    public EndPoint RemoteEndPoint => _client?.Client?.RemoteEndPoint;

    public EndPoint LocalEndPoint => _client?.Client?.LocalEndPoint;

    public async Task<bool> ConnectAsync(string host, int port)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkSocket));

        try
        {
            lock (_lockObject)
            {
                if (_isConnected)
                    return true;
            }

            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();

            lock (_lockObject)
            {
                _isConnected = true;
            }

            Connected?.Invoke(this, new SocketEventArgs($"Connected to {RemoteEndPoint?.ToString() ?? "unknown endpoint"}"));
            StartReceiving();
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new SocketErrorEventArgs(this, ex));
            return false;
        }
    }

    public async Task<bool> ConnectAsync(IPAddress address, int port)
    {
        return await ConnectAsync(address.ToString(), port);
    }

    public async Task<bool> ConnectAsync(IPEndPoint endPoint)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkSocket));

        try
        {
            lock (_lockObject)
            {
                if (_isConnected)
                    return true;
            }

            await _client.ConnectAsync(endPoint.Address, endPoint.Port);
            _stream = _client.GetStream();

            lock (_lockObject)
            {
                _isConnected = true;
            }

            Connected?.Invoke(this, new SocketEventArgs($"Connected to {RemoteEndPoint?.ToString() ?? "unknown endpoint"}"));
            StartReceiving();
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new SocketErrorEventArgs(this, ex));
            return false;
        }
    }

    public async Task<int> SendAsync(byte[] data)
    {
        return await SendAsync(data, 0, data.Length);
    }

    public async Task<int> SendAsync(byte[] data, int offset, int count)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkSocket));

        if (!IsConnected)
            throw new InvalidOperationException("Socket is not connected");

        try
        {
            await _stream.WriteAsync(data, offset, count);
            return count;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new SocketErrorEventArgs(this, ex));
            throw;
        }
    }

    public async Task<int> ReceiveAsync(byte[] buffer)
    {
        return await ReceiveAsync(buffer, 0, buffer.Length);
    }

    public async Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkSocket));

        if (!IsConnected)
            throw new InvalidOperationException("Socket is not connected");

        try
        {
            return await _stream.ReadAsync(buffer, offset, count);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new SocketErrorEventArgs(this, ex));
            throw;
        }
    }

    public void Disconnect()
    {
        if (_isDisposed)
            return;

        lock (_lockObject)
        {
            if (!_isConnected)
                return;

            _isConnected = false;
        }

        try
        {
            _client?.Close();
            _stream?.Close();
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new SocketErrorEventArgs(this, ex));
        }
        finally
        {
            Disconnected?.Invoke(this, new SocketEventArgs($"Disconnected from {RemoteEndPoint?.ToString() ?? "unknown endpoint"}"));
        }
    }

    private async void StartReceiving()
    {
        var buffer = new byte[4096];

        try
        {
            while (IsConnected)
            {
                var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }

                var data = new byte[bytesRead];
                Array.Copy(buffer, data, bytesRead);
                DataReceived?.Invoke(this, new SocketDataEventArgs(data));
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new SocketErrorEventArgs(this, ex));
        }
        finally
        {
            Disconnect();
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        Disconnect();
        _client?.Dispose();
        _stream?.Dispose();
    }
}
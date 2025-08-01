using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DarkAges.Library.Network;

public class NetworkConnection : IDisposable
{
    private Socket? _socket;
    private bool _isDisposed;
    private readonly byte[] _receiveBuffer = new byte[8192];
    private readonly byte[] _sendBuffer = new byte[8192];

    public event EventHandler<SocketDataEventArgs>? DataReceived;
    public event EventHandler<SocketDataEventArgs>? DataSent;
    public event EventHandler<SocketEventArgs>? Connected;
    public event EventHandler<SocketEventArgs>? Disconnected;
    public event EventHandler<NetworkErrorEventArgs>? Error;

    public bool IsConnected => _socket is { Connected: true };

    public async Task ConnectAsync(string address, int port)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkConnection));

        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _socket.ConnectAsync(address, port);
            Connected?.Invoke(this, new SocketEventArgs($"Connected to {address}:{port}"));
            StartReceiving();
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new NetworkErrorEventArgs(new NetworkError(NetworkErrorCode.ConnectionFailed, ex)));
            throw;
        }
    }

    public void Disconnect()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkConnection));

        try
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
                Disconnected?.Invoke(this, new SocketEventArgs("Disconnected"));
            }
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new NetworkErrorEventArgs(new NetworkError(NetworkErrorCode.DisconnectionFailed, ex)));
            throw;
        }
    }

    public async Task SendAsync(byte[] data)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkConnection));

        if (_socket == null)
            throw new InvalidOperationException("Not connected");

        try
        {
            await _socket.SendAsync(data, SocketFlags.None);
            DataSent?.Invoke(this, new SocketDataEventArgs(data));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new NetworkErrorEventArgs(new NetworkError(NetworkErrorCode.SendFailed, ex)));
            throw;
        }
    }

    private void StartReceiving()
    {
        if (_socket == null)
            return;

        try
        {
            _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, OnDataReceived, null);
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new NetworkErrorEventArgs(new NetworkError(NetworkErrorCode.ReceiveFailed, ex)));
        }
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        if (_socket == null)
            return;

        try
        {
            var bytesRead = _socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                var data = new byte[bytesRead];
                Array.Copy(_receiveBuffer, data, bytesRead);
                DataReceived?.Invoke(this, new SocketDataEventArgs(data));
                StartReceiving();
            }
            else
            {
                Disconnect();
            }
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new NetworkErrorEventArgs(new NetworkError(NetworkErrorCode.ReceiveFailed, ex)));
            Disconnect();
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        Disconnect();
    }
}
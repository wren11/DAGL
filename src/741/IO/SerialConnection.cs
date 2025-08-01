using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace DarkAges.Library.IO;

/// <summary>
/// Handles serial port communication
/// </summary>
public class SerialConnection : IDisposable
{
    private const int DEFAULT_BUFFER_SIZE = 98304;
    private const int DEFAULT_TIMEOUT = 1000;
    private const int MAX_PORT_NUMBER = 10;

    private SerialPort serialPort;
    private byte[] receiveBuffer;
    private byte[] sendBuffer;
    private int receiveBufferSize;
    private int sendBufferSize;
    private bool isConnected;
    private bool isDisposed;
    private int portNumber;
    private int baudRate;
    private int dataBits;
    private Parity parity;
    private StopBits stopBits;
    private bool useFlowControl;
    private bool useDtrControl;
    private bool useRtsControl;

    // Statistics
    private long bytesReceived;
    private long bytesSent;
    private int connectionAttempts;
    private DateTime lastActivity;

    // Events
    public event Action Connected;
    public event Action Disconnected;
    public event Action<SerialError> ConnectionError;
    public event Action<byte[]> DataReceived;
    public event Action<int> DataSent;

    public SerialConnection()
    {
        InitializeConnection();
    }

    public SerialConnection(int portNumber) : this()
    {
        SetPortNumber(portNumber);
    }

    private void InitializeConnection()
    {
        serialPort = null;
        receiveBuffer = new byte[DEFAULT_BUFFER_SIZE];
        sendBuffer = new byte[DEFAULT_BUFFER_SIZE];
        receiveBufferSize = 0;
        sendBufferSize = 0;
        isConnected = false;
        isDisposed = false;
        portNumber = 1;
        baudRate = 9600;
        dataBits = 8;
        parity = Parity.None;
        stopBits = StopBits.One;
        useFlowControl = false;
        useDtrControl = true;
        useRtsControl = true;

        // Initialize statistics
        bytesReceived = 0;
        bytesSent = 0;
        connectionAttempts = 0;
        lastActivity = DateTime.Now;
    }

    public void SetPortNumber(int portNumber)
    {
        if (portNumber < 1 || portNumber > MAX_PORT_NUMBER)
            throw new ArgumentOutOfRangeException(nameof(portNumber), $"Port number must be between 1 and {MAX_PORT_NUMBER}");

        this.portNumber = portNumber;
    }

    public void SetBaudRate(int baudRate)
    {
        this.baudRate = baudRate;
    }

    public void SetDataBits(int dataBits)
    {
        if (dataBits < 5 || dataBits > 8)
            throw new ArgumentOutOfRangeException(nameof(dataBits), "Data bits must be between 5 and 8");

        this.dataBits = dataBits;
    }

    public void SetParity(Parity parity)
    {
        this.parity = parity;
    }

    public void SetStopBits(StopBits stopBits)
    {
        this.stopBits = stopBits;
    }

    public void SetFlowControl(bool enabled)
    {
        useFlowControl = enabled;
    }

    public void SetDtrControl(bool enabled)
    {
        useDtrControl = enabled;
    }

    public void SetRtsControl(bool enabled)
    {
        useRtsControl = enabled;
    }

    public async Task<bool> ConnectAsync()
    {
        return await ConnectAsync(CancellationToken.None);
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(SerialConnection));

        if (isConnected)
            return true;

        connectionAttempts = 0;

        while (connectionAttempts < MAX_PORT_NUMBER)
        {
            try
            {
                connectionAttempts++;
                    
                // Create serial port
                var portName = $"COM{portNumber}";
                serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                    
                // Configure serial port
                ConfigureSerialPort();
                    
                // Open connection
                serialPort.Open();
                    
                // Test connection
                if (!TestConnection())
                {
                    throw new SerialException("Connection test failed");
                }

                isConnected = true;
                lastActivity = DateTime.Now;
                Connected?.Invoke();

                // Start receiving data
                _ = Task.Run(ReceiveLoop, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                CleanupConnection();
                    
                if (connectionAttempts >= MAX_PORT_NUMBER)
                {
                    ConnectionError?.Invoke(new SerialError
                    {
                        ErrorCode = SerialErrorCode.ConnectionFailed,
                        Message = $"Failed to connect to COM{portNumber} after {MAX_PORT_NUMBER} attempts: {ex.Message}",
                        Exception = ex
                    });
                    return false;
                }

                // Try next port
                portNumber = (portNumber % MAX_PORT_NUMBER) + 1;
                    
                // Wait before retry
                await Task.Delay(100, cancellationToken);
            }
        }

        return false;
    }

    private void ConfigureSerialPort()
    {
        if (serialPort == null)
            return;

        // Set timeouts
        serialPort.ReadTimeout = DEFAULT_TIMEOUT;
        serialPort.WriteTimeout = DEFAULT_TIMEOUT;

        // Set buffer sizes
        serialPort.ReadBufferSize = DEFAULT_BUFFER_SIZE;
        serialPort.WriteBufferSize = DEFAULT_BUFFER_SIZE;

        // Set flow control
        serialPort.Handshake = useFlowControl ? Handshake.RequestToSend : Handshake.None;

        // Set DTR and RTS control
        serialPort.DtrEnable = useDtrControl;
        serialPort.RtsEnable = useRtsControl;

        // Set additional properties
        serialPort.DiscardNull = false;
        serialPort.ParityReplace = 63;
    }

    private bool TestConnection()
    {
        try
        {
            // Clear any pending data
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();

            // Test if port is accessible
            if (!serialPort.IsOpen)
                return false;

            // Reset statistics
            receiveBufferSize = 0;
            sendBufferSize = 0;
            bytesReceived = 0;
            bytesSent = 0;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> SendAsync(byte[] data)
    {
        if (isDisposed || !isConnected || serialPort == null)
            throw new InvalidOperationException("Connection not established");

        try
        {
            await serialPort.BaseStream.WriteAsync(data, 0, data.Length);
                
            bytesSent += data.Length;
            lastActivity = DateTime.Now;
                
            DataSent?.Invoke(data.Length);
                
            return data.Length;
        }
        catch (Exception ex)
        {
            HandleConnectionError(ex);
            throw;
        }
    }

    public async Task<byte[]> ReceiveAsync(int count)
    {
        if (isDisposed || !isConnected || serialPort == null)
            throw new InvalidOperationException("Connection not established");

        try
        {
            var buffer = new byte[count];
            var bytesRead = await serialPort.BaseStream.ReadAsync(buffer, 0, count);
                
            if (bytesRead == 0)
            {
                // No data available
                return [];
            }

            var result = new byte[bytesRead];
            Array.Copy(buffer, result, bytesRead);
                
            bytesReceived += bytesRead;
            lastActivity = DateTime.Now;
                
            return result;
        }
        catch (Exception ex)
        {
            HandleConnectionError(ex);
            throw;
        }
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isConnected && !isDisposed && serialPort != null)
            {
                // Check if data is available
                if (serialPort.BytesToRead > 0)
                {
                    var bytesRead = await serialPort.BaseStream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
                        
                    if (bytesRead > 0)
                    {
                        var data = new byte[bytesRead];
                        Array.Copy(receiveBuffer, data, bytesRead);
                            
                        bytesReceived += bytesRead;
                        lastActivity = DateTime.Now;

                        DataReceived?.Invoke(data);
                    }
                }
                else
                {
                    // Wait a bit before checking again
                    await Task.Delay(10);
                }
            }
        }
        catch (Exception ex)
        {
            HandleConnectionError(ex);
        }
        finally
        {
            if (isConnected)
            {
                Disconnect();
            }
        }
    }

    private void HandleConnectionError(Exception ex)
    {
        var error = new SerialError
        {
            ErrorCode = SerialErrorCode.ConnectionLost,
            Message = ex.Message,
            Exception = ex
        };

        ConnectionError?.Invoke(error);
    }

    public void Disconnect()
    {
        if (!isConnected)
            return;

        isConnected = false;
        CleanupConnection();
        Disconnected?.Invoke();
    }

    private void CleanupConnection()
    {
        if (serialPort != null)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                serialPort.Dispose();
            }
            catch
            {
                // Ignore cleanup errors
            }
            serialPort = null;
        }
    }

    public bool IsConnected()
    {
        return isConnected && serialPort != null && serialPort.IsOpen;
    }

    public int GetPortNumber()
    {
        return portNumber;
    }

    public int GetBaudRate()
    {
        return baudRate;
    }

    public int GetDataBits()
    {
        return dataBits;
    }

    public Parity GetParity()
    {
        return parity;
    }

    public StopBits GetStopBits()
    {
        return stopBits;
    }

    public SerialStatistics GetStatistics()
    {
        return new SerialStatistics
        {
            BytesReceived = bytesReceived,
            BytesSent = bytesSent,
            ConnectionAttempts = connectionAttempts,
            LastActivity = lastActivity,
            IsConnected = isConnected,
            PortNumber = portNumber,
            BaudRate = baudRate
        };
    }

    public void ResetStatistics()
    {
        bytesReceived = 0;
        bytesSent = 0;
        connectionAttempts = 0;
        lastActivity = DateTime.Now;
    }

    public void FlushBuffers()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                Disconnect();
            }

            isDisposed = true;
        }
    }
}
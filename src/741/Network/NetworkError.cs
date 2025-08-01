using System;

namespace DarkAges.Library.Network;

public class NetworkError
{
    public NetworkErrorCode Code { get; }
    public string Message { get; }
    public Exception? Exception { get; }

    public NetworkError(NetworkErrorCode code, Exception? exception = null)
    {
        Code = code;
        Message = GetErrorMessage(code);
        Exception = exception;
    }

    private static string GetErrorMessage(NetworkErrorCode code)
    {
        return code switch
        {
            NetworkErrorCode.ConnectionFailed => "Failed to establish network connection",
            NetworkErrorCode.DisconnectionFailed => "Failed to disconnect from network",
            NetworkErrorCode.SendFailed => "Failed to send data",
            NetworkErrorCode.ReceiveFailed => "Failed to receive data",
            NetworkErrorCode.DecryptionFailed => "Failed to decrypt data",
            NetworkErrorCode.EncryptionFailed => "Failed to encrypt data",
            NetworkErrorCode.InvalidData => "Invalid data received",
            NetworkErrorCode.InvalidState => "Invalid network state",
            NetworkErrorCode.Timeout => "Network operation timed out",
            _ => "Unknown network error"
        };
    }
}
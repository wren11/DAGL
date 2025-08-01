namespace DarkAges.Library.IO;

/// <summary>
/// Serial error codes
/// </summary>
public enum SerialErrorCode
{
    ConnectionFailed,
    ConnectionLost,
    Timeout,
    InvalidData,
    PortNotFound,
    AccessDenied,
    DeviceInUse
}
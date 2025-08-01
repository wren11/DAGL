namespace DarkAges.Library.Network;

public enum NetworkErrorCode
{
    ConnectionFailed,
    DisconnectionFailed,
    SendFailed,
    ReceiveFailed,
    DecryptionFailed,
    EncryptionFailed,
    InvalidData,
    InvalidState,
    Timeout
}
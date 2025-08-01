namespace DarkAges.Library.Network;

/// <summary>
/// Network error recovery strategies
/// </summary>
public enum NetworkErrorRecovery
{
    None,
    RetryWithDelay,
    WaitAndRetry,
    ChangeProtocol,
    ChangeSocketType,
    ChangeAddressFamily,
    InitializeWinsock
}
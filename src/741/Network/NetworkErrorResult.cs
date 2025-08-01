namespace DarkAges.Library.Network;

public class NetworkErrorResult
{
    public NetworkError Error { get; }
    public NetworkErrorSeverity Severity { get; }

    public NetworkErrorResult(NetworkError error, NetworkErrorSeverity severity)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
        Severity = severity;
    }
}
namespace DarkAges.Library.Network;

/// <summary>
/// Authentication result
/// </summary>
public class AuthenticationResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = "";
    public int ErrorCode { get; set; }
}
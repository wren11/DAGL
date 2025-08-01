namespace DarkAges.Library.Network;

public class LoginResponseMessage : NetworkMessage
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SessionToken { get; set; }
    public DateTime? LastLogin { get; set; }

    public LoginResponseMessage()
    {
        MessageType = "LoginResponse";
    }
}
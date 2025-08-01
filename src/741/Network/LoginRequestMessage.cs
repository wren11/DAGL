namespace DarkAges.Library.Network;

public class LoginRequestMessage : NetworkMessage
{
    public string Username { get; set; } = "";
    public string HashedPassword { get; set; } = "";

    public LoginRequestMessage()
    {
        MessageType = "LoginRequest";
    }
}
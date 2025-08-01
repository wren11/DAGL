namespace DarkAges.Library.Network;

public class CreateAccountRequestMessage : NetworkMessage
{
    public string Username { get; set; } = "";
    public string HashedPassword { get; set; } = "";
    public string Email { get; set; } = "";

    public CreateAccountRequestMessage()
    {
        MessageType = "CreateAccountRequest";
    }
}
namespace DarkAges.Library.Network;

public class CreateAccountResponseMessage : NetworkMessage
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? UserId { get; set; }

    public CreateAccountResponseMessage()
    {
        MessageType = "CreateAccountResponse";
    }
}
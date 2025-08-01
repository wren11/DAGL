namespace DarkAges.Library.UI.NPC;

public class PursuitMessage
{
    public byte MessageType { get; set; }
    // Other properties from the message structure will go here
    public string Text { get; set; } = string.Empty;
    public short FaceImage { get; set; }
    //...
}
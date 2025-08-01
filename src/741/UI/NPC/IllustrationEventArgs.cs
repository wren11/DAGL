namespace DarkAges.Library.UI.NPC;

public class IllustrationEventArgs(int npcId, ImagePane image) : EventArgs
{
    public int NpcId { get; } = npcId;
    public ImagePane Image { get; } = image;
}
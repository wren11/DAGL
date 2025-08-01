namespace DarkAges.Library.UI.NPC;

public class NPCIndirectTextMenu : NPCTextMenu
{
    private int _indirectTextId;
    private string _indirectTextKey;

    public void SetIndirectText(int textId, string key = "")
    {
        _indirectTextId = textId;
        _indirectTextKey = key ?? "";
        LoadIndirectText();
    }

    private void LoadIndirectText()
    {
        try
        {
            // For now, use a placeholder since StringTable doesn't exist
            var text = $"Indirect text: ID={_indirectTextId}, Key={_indirectTextKey}";
            SetMessage(text);
        }
        catch
        {
            SetMessage($"Indirect text not found: ID={_indirectTextId}, Key={_indirectTextKey}");
        }
    }
}
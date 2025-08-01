using DarkAges.Library.Core;

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
            var key = $"indirect_text_{_indirectTextId}";
            if (!string.IsNullOrEmpty(_indirectTextKey))
            {
                key += $"_{_indirectTextKey}";
            }
            var text = StringTable.GetString(key);
            SetMessage(text);
        }
        catch
        {
            SetMessage($"Indirect text not found: ID={_indirectTextId}, Key={_indirectTextKey}");
        }
    }
}
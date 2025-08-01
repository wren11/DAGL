namespace DarkAges.Library.UI.NPC;

public class NPCMenuItem(string text, Action action = null, int id = 0)
{
    public string Text { get; set; } = text ?? throw new ArgumentNullException(nameof(text));
    public int Id { get; set; } = id;
    public Action Action { get; set; } = action;
    public bool IsEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;

    public virtual void Execute()
    {
        if (IsEnabled && Action != null)
        {
            Action();
        }
    }
}
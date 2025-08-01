namespace DarkAges.Library.UI.NPC;

public class TextInputEventArgs(string text) : EventArgs
{
    public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));
}
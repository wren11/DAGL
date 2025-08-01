namespace DarkAges.Library.UI;

public class MenuItem
{
    public string Text { get; set; } = "";
    public int Id { get; set; } = -1;
    public bool Enabled { get; set; } = true;
    public Action Action { get; set; } = null;
}
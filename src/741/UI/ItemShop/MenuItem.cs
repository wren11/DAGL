namespace DarkAges.Library.UI.ItemShop;

public class MenuItem
{
    public string Text { get; set; }
    public ushort Id { get; set; }

    public override string ToString()
    {
        return Text;
    }
}
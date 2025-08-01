namespace DarkAges.Library.UI.ItemShop;

public class ServerItemMenuDialog2 : DialogPane
{
    public event EventHandler<ItemTransactionEventArgs> ItemTransactionRequested = delegate { };

    public ServerItemMenuDialog2(byte[] packet)
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        Size = new System.Drawing.Size(400, 300);
        Position = new System.Drawing.Point(120, 90);

        var titleLabel = new TextEditControlPane("Item Shop", new System.Drawing.Rectangle(10, 10, 380, 20), true);
        AddChild(titleLabel);
    }
}
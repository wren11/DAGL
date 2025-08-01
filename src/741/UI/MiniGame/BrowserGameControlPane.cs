using System.Drawing;

namespace DarkAges.Library.UI.MiniGame;

public class BrowserGameControlPane : AbstractGameControlPane
{
    private TextButtonExControlPane _browserButton;

    public BrowserGameControlPane()
    {
        _browserButton = new TextButtonExControlPane("Browser");
        _browserButton.Position = new Point(400, 210);
        AddChild(_browserButton);
    }
}
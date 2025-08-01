using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleSendResultPane : ControlPane
{
    private TextButtonExControlPane _sendButton;
    private TextButtonExControlPane _cancelButton;

    public PuzzleSendResultPane()
    {
        _sendButton = new TextButtonExControlPane("Send Result");
        _cancelButton = new TextButtonExControlPane("Cancel");

        _sendButton.Position = new Point(200, 150);
        _cancelButton.Position = new Point(200, 180);

        AddChild(_sendButton);
        AddChild(_cancelButton);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Send Result", 200, 100, Color.Black);
            
        base.Render(spriteBatch);
    }
}
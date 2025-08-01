using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public abstract class InfoPane : ControlPane
{
    protected string _title;
    protected bool _isVisible;

    public virtual void Show()
    {
        _isVisible = true;
        IsVisible = true;
    }

    public virtual void Hide()
    {
        _isVisible = false;
        IsVisible = false;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!_isVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        var backgroundRect = new Rectangle(50, 50, 500, 400);
        spriteBatch.DrawRectangle(backgroundRect, Color.White);
        spriteBatch.DrawRectangle(backgroundRect, Color.Black);

        //spriteBatch.DrawString(font, _title, 250, 70, Color.Black);
            
        RenderContent(spriteBatch);
    }

    protected abstract void RenderContent(SpriteBatch spriteBatch);
}
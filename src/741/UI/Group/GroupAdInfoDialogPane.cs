using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Group;

public class GroupAdInfoDialogPane : ControlPane
{
    private GroupAd _currentAd;
    private TextButtonExControlPane _closeButton;

    public GroupAdInfoDialogPane()
    {
        _closeButton = new TextButtonExControlPane("Close");
        _closeButton.Position = new Point(200, 200);
        _closeButton.Click += (s, e) => Hide();
        AddChild(_closeButton);
    }

    public void SetAd(GroupAd ad)
    {
        _currentAd = ad;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        var backgroundRect = new Rectangle(50, 50, 400, 200);
        spriteBatch.DrawRectangle(backgroundRect, Color.White);
        spriteBatch.DrawRectangle(backgroundRect, Color.Black);

        //spriteBatch.DrawString(font, "Group Advertisement", 200, 70, Color.Black);

        if (_currentAd != null)
        {
            //spriteBatch.DrawString(font, $"Group: {_currentAd.GroupName}", 70, 100, Color.Black);
            //spriteBatch.DrawString(font, $"Message: {_currentAd.Message}", 70, 130, Color.Black);
            //spriteBatch.DrawString(font, $"Posted: {_currentAd.PostedDate}", 70, 160, Color.Black);
        }
            
        base.Render(spriteBatch);
    }
}
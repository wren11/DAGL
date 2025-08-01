using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NexonClub;

public class NexonClubAuthAlertPane : ControlPane
{
    private TextButtonExControlPane _loginButton;
    private TextButtonExControlPane _cancelButton;

    public event EventHandler AuthenticationRequested;

    public NexonClubAuthAlertPane()
    {
        InitializeControls();
    }

    private void InitializeControls()
    {
        _loginButton = new TextButtonExControlPane("Login to Nexon Club");
        _cancelButton = new TextButtonExControlPane("Cancel");

        _loginButton.Position = new Point(150, 150);
        _cancelButton.Position = new Point(250, 150);

        _loginButton.Click += (s, e) => AuthenticationRequested?.Invoke(this, e);
        _cancelButton.Click += (s, e) => Hide();

        AddChild(_loginButton);
        AddChild(_cancelButton);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        var backgroundRect = new Rectangle(50, 50, 400, 200);
        spriteBatch.DrawRectangle(backgroundRect, Color.White);
        spriteBatch.DrawRectangle(backgroundRect, Color.Black);

        //spriteBatch.DrawString(font, "Nexon Club Authentication", 200, 70, Color.Black);
        //spriteBatch.DrawString(font, "You need to login to Nexon Club to access this feature.", 70, 100, Color.Black);
            
        base.Render(spriteBatch);
    }
}
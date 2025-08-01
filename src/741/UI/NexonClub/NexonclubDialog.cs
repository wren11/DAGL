using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NexonClub;

public class NexonclubDialog : ControlPane
{
    private TextEditControlPane _usernameField = null!;
    private TextEditControlPane _passwordField = null!;
    private TextButtonExControlPane _loginButton = null!;
    private TextButtonExControlPane _cancelButton = null!;
    private TextButtonExControlPane _registerButton = null!;

    public event EventHandler<bool> LoginCompleted = delegate { };

    public NexonclubDialog()
    {
        InitializeControls();
    }

    private void InitializeControls()
    {
        _usernameField = new TextEditControlPane("", new Rectangle(150, 100, 200, 25));
        _passwordField = new TextEditControlPane("", new Rectangle(150, 140, 200, 25));
        _loginButton = new TextButtonExControlPane("Login");
        _cancelButton = new TextButtonExControlPane("Cancel");
        _registerButton = new TextButtonExControlPane("Register");

        _loginButton.Position = new Point(150, 180);
        _cancelButton.Position = new Point(250, 180);
        _registerButton.Position = new Point(350, 180);

        _loginButton.Click += (s, e) => AttemptLogin();
        _cancelButton.Click += (s, e) => CancelLogin();
        _registerButton.Click += (s, e) => OpenRegistration();

        AddChild(_usernameField);
        AddChild(_passwordField);
        AddChild(_loginButton);
        AddChild(_cancelButton);
        AddChild(_registerButton);
    }

    private void AttemptLogin()
    {
        var username = _usernameField.Text;
        var password = _passwordField.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            LoginCompleted?.Invoke(this, false);
            return;
        }

        LoginCompleted?.Invoke(this, true);
    }

    private void CancelLogin()
    {
        LoginCompleted?.Invoke(this, false);
    }

    private void OpenRegistration()
    {
        LoginCompleted?.Invoke(this, false);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        var backgroundRect = new Rectangle(50, 50, 450, 250);
        spriteBatch.DrawRectangle(backgroundRect, Color.White);
        spriteBatch.DrawRectangle(backgroundRect, Color.Black);

        //spriteBatch.DrawString(font, "Nexon Club Login", 225, 70, Color.Black);
        //spriteBatch.DrawString(font, "Username:", 80, 105, Color.Black);
        //spriteBatch.DrawString(font, "Password:", 80, 145, Color.Black);
            
        base.Render(spriteBatch);
    }
}
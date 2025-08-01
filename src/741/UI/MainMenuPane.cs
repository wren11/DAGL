using DarkAges.Library.Graphics;
using DarkAges.Library.Core;

namespace DarkAges.Library.UI;

public class MainMenuPane : Pane
{
    private readonly ButtonControlPane _loginButton;
    private readonly ButtonControlPane _createAccountButton;
    private readonly ButtonControlPane _creditsButton;
    private readonly ButtonControlPane _homepageButton;
    private readonly ButtonControlPane _exitButton;
    private readonly LogoPane _logoPane;
    private readonly DarkAgesApplication _application;

    public MainMenuPane()
    {
        // Create logo pane
        _logoPane = new LogoPane();
        AddChild(_logoPane);

        // Create buttons
        var buttonWidth = 200;
        var buttonHeight = 40;
        var buttonSpacing = 10;
        var startY = 300;

        _loginButton = new ButtonControlPane
        {
            Text = "Login",
            Bounds = new System.Drawing.Rectangle(
                (Bounds.Width - buttonWidth) / 2,
                startY,
                buttonWidth,
                buttonHeight)
        };
        _loginButton.Click += (s, e) => _application?.ShowLoginDialog();
        AddChild(_loginButton);

        _createAccountButton = new ButtonControlPane
        {
            Text = "Create Account",
            Bounds = new System.Drawing.Rectangle(
                (Bounds.Width - buttonWidth) / 2,
                startY + buttonHeight + buttonSpacing,
                buttonWidth,
                buttonHeight)
        };
        _createAccountButton.Click += (s, e) => _application?.ShowCreateUserDialog();
        AddChild(_createAccountButton);

        _creditsButton = new ButtonControlPane
        {
            Text = "Credits",
            Bounds = new System.Drawing.Rectangle(
                (Bounds.Width - buttonWidth) / 2,
                startY + (buttonHeight + buttonSpacing) * 2,
                buttonWidth,
                buttonHeight)
        };
        _creditsButton.Click += (s, e) => _application?.ShowCreditPane();
        AddChild(_creditsButton);

        _homepageButton = new ButtonControlPane
        {
            Text = "Homepage",
            Bounds = new System.Drawing.Rectangle(
                (Bounds.Width - buttonWidth) / 2,
                startY + (buttonHeight + buttonSpacing) * 3,
                buttonWidth,
                buttonHeight)
        };
        _homepageButton.Click += (s, e) => _application?.OpenHomepage();
        AddChild(_homepageButton);

        _exitButton = new ButtonControlPane
        {
            Text = "Exit",
            Bounds = new System.Drawing.Rectangle(
                (Bounds.Width - buttonWidth) / 2,
                startY + (buttonHeight + buttonSpacing) * 4,
                buttonWidth,
                buttonHeight)
        };
        _exitButton.Click += (s, e) => _application?.Exit();
        AddChild(_exitButton);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || spriteBatch == null) return;

        // Draw background
        spriteBatch.Clear(System.Drawing.Color.Black);

        // Draw children (logo and buttons)
        base.Render(spriteBatch);
    }
}
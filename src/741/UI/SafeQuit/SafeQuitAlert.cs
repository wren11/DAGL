using System;
using DarkAges.Library.Graphics;
using System.Drawing;
using Size = System.Drawing.Size;

namespace DarkAges.Library.UI.SafeQuit;

public class SafeQuitAlert : ControlPane
{
    private TextButtonExControlPane _yesButton;
    private TextButtonExControlPane _noButton;
    private TextButtonExControlPane _cancelButton;
    private ImagePane _backgroundImage;
    private Rectangle _backgroundRect;
    private bool _isQuitting;

    public event EventHandler<bool> QuitConfirmed;

    public SafeQuitAlert()
    {
        InitializeControls();
        LoadLayout();
    }

    private void InitializeControls()
    {
        _yesButton = new TextButtonExControlPane("Yes");
        _noButton = new TextButtonExControlPane("No");
        _cancelButton = new TextButtonExControlPane("Cancel");

        _yesButton.Click += (s, e) => ConfirmQuit(true);
        _noButton.Click += (s, e) => ConfirmQuit(false);
        _cancelButton.Click += (s, e) => Hide();

        AddChild(_yesButton);
        AddChild(_noButton);
        AddChild(_cancelButton);
    }

    private void LoadLayout()
    {
        try
        {
            // For now, use hardcoded layout since LayoutFileParser constructor is different
            _backgroundRect = new Rectangle(100, 100, 400, 200);
                
            var yesButtonRect = new Rectangle(150, 150, 80, 30);
            var noButtonRect = new Rectangle(250, 150, 80, 30);
            var cancelButtonRect = new Rectangle(350, 150, 80, 30);
                
            _yesButton.Position = new Point(yesButtonRect.X, yesButtonRect.Y);
            _yesButton.Size = new Size(yesButtonRect.Width, yesButtonRect.Height);
                
            _noButton.Position = new Point(noButtonRect.X, noButtonRect.Y);
            _noButton.Size = new Size(noButtonRect.Width, noButtonRect.Height);
                
            _cancelButton.Position = new Point(cancelButtonRect.X, cancelButtonRect.Y);
            _cancelButton.Size = new Size(cancelButtonRect.Width, cancelButtonRect.Height);
        }
        catch (Exception ex)
        {
            // Fallback to default layout
            _backgroundRect = new Rectangle(100, 100, 400, 200);
                
            _yesButton.Position = new Point(150, 150);
            _yesButton.Size = new Size(80, 30);
                
            _noButton.Position = new Point(250, 150);
            _noButton.Size = new Size(80, 30);
                
            _cancelButton.Position = new Point(350, 150);
            _cancelButton.Size = new Size(80, 30);
        }
    }

    private void ConfirmQuit(bool confirmed)
    {
        _isQuitting = confirmed;
        QuitConfirmed?.Invoke(this, confirmed);
        Hide();
    }

    public void ShowQuitDialog()
    {
        Show();
        _isQuitting = false;
    }

    public bool IsQuitting => _isQuitting;

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        if (_backgroundImage != null)
        {
            //spriteBatch.DrawImage(_backgroundImage, _backgroundRect.X, _backgroundRect.Y);
        }
        else
        {
            spriteBatch.DrawRectangle(_backgroundRect, Color.White);
            spriteBatch.DrawRectangle(_backgroundRect, Color.Black);
        }

        //spriteBatch.DrawString(font, "Are you sure you want to quit?", _backgroundRect.X + 50, _backgroundRect.Y + 50, Color.Black);
        //spriteBatch.DrawString(font, "Any unsaved progress will be lost.", _backgroundRect.X + 50, _backgroundRect.Y + 80, Color.Black);
            
        base.Render(spriteBatch);
    }
}
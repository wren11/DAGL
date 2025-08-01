using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.MiniGame;

public class GameDialogPane : ControlPane
{
    private string _title;
    private string _message;
    private TextButtonExControlPane _okButton;
    private TextButtonExControlPane _cancelButton;
    private bool _showCancelButton;

    public event EventHandler<bool> DialogResult;

    public GameDialogPane(string title, string message, bool showCancelButton = false)
    {
        _title = title;
        _message = message;
        _showCancelButton = showCancelButton;
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        _okButton = new TextButtonExControlPane("OK");
        _okButton.Position = new Point(150, 120);
        _okButton.Click += (s, e) => DialogResult?.Invoke(this, true);

        AddChild(_okButton);

        if (_showCancelButton)
        {
            _cancelButton = new TextButtonExControlPane("Cancel");
            _cancelButton.Position = new Point(250, 120);
            _cancelButton.Click += (s, e) => DialogResult?.Invoke(this, false);
            AddChild(_cancelButton);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        var backgroundRect = new Rectangle(50, 50, 400, 150);
        //spriteBatch.DrawRectangle(backgroundRect, Color.White);
        //spriteBatch.DrawRectangle(backgroundRect, Color.Black, 2);

        //spriteBatch.DrawString(font, _title, 200, 70, Color.Black);
        //spriteBatch.DrawString(font, _message, 70, 100, Color.Black);
            
        base.Render(spriteBatch);
    }
}
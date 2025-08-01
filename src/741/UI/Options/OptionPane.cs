using System.Drawing;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI.Options;

public class OptionPane : ControlPane
{
    private readonly List<TextButtonExControlPane> _optionButtons = [];
    private TextButtonExControlPane _closeButton = null!;
    private TextButtonExControlPane _friendsButton = null!;
    private ImagePane _backgroundImage = null!;
    private Rectangle _backgroundRect;

    public event EventHandler<string> SettingChanged = delegate { };

    public OptionPane()
    {
        InitializeControls();
        LoadLayout();
    }

    private void InitializeControls()
    {
        _closeButton = new TextButtonExControlPane("CLOSE");
        _friendsButton = new TextButtonExControlPane("Friends");

        _closeButton.Click += (s, e) => Hide();
        _friendsButton.Click += (s, e) => ShowFriendsList();

        AddChild(_closeButton);
        AddChild(_friendsButton);
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_noptdlg.txt");
                
            var backgroundName = layout.GetString("Noname", "Noname");
            _backgroundImage = new ImagePane();
            _backgroundImage.SetImage(ImageLoader.LoadImage(backgroundName), null);
                
            _backgroundRect = layout.GetRect("Noname");
                
            _closeButton.Bounds = layout.GetRect("CLOSE");
            _friendsButton.Bounds = layout.GetRect("Friends");
        }
        catch
        {
            _backgroundRect = new Rectangle(50, 50, 400, 300);
            _closeButton.Position = new Point(350, 320);
            _friendsButton.Position = new Point(100, 320);
        }
    }

    private void ShowFriendsList()
    {
        SettingChanged?.Invoke(this, "Friends");
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        if (_backgroundImage != null)
        {
            _backgroundImage.Render(spriteBatch);
        }
        else
        {
            spriteBatch.DrawRectangle(_backgroundRect, System.Drawing.Color.White);
            spriteBatch.DrawRectangle(_backgroundRect, System.Drawing.Color.Black);
        }

        base.Render(spriteBatch);
    }
}
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;
using System.Drawing;

namespace DarkAges.Library.UI;

public class FishingDialogPane : ControlPane
{
    private ImagePane _backgroundImage;
    private TextButtonExControlPane _closeButton;
    private LabelControlPane _statusLabel;

    public FishingDialogPane()
    {
        InitializeControls();
        LoadLayout();
    }

    private void InitializeControls()
    {
        _closeButton = new TextButtonExControlPane("Close");
        
        // Create a default font for the label
        var defaultFont = new SimpleFont("Arial", 12);
        _statusLabel = new LabelControlPane(defaultFont);
        _statusLabel.Text = "Fishing...";

        _closeButton.Click += (s, e) => Hide();

        AddChild(_closeButton);
        AddChild(_statusLabel);
    }

    private void Hide()
    {
        IsVisible = false;
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("fsh_dlg.txt");

            var backgroundName = layout.GetString("Background", "fishing_bg");
            _backgroundImage = new ImagePane();
            var loadedImage = ImageLoader.LoadImage(backgroundName);
            _backgroundImage.SetImage(loadedImage, null);

            _closeButton.Bounds = layout.GetRect("CloseButton");
            _statusLabel.Bounds = layout.GetRect("StatusLabel");
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error loading layout: {ex.Message}");
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _backgroundImage?.Render(spriteBatch);

        base.Render(spriteBatch);
    }
}
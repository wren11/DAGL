using DarkAges.Library.Graphics;
using System.Drawing;

namespace DarkAges.Library.UI;

public class ItemPane : ControlPane
{
    private readonly FrameInfo _frameInfo;
    private readonly IndexedImage _image;
    private readonly Rectangle _bounds;
    private readonly DarkAges.Library.Graphics.Vector2 _position;
    private bool _isSelected;

    public short ItemId { get; set; }
    public string ItemName { get; set; }

    public ItemPane(FrameInfo frameInfo, IndexedImage image, Rectangle bounds)
    {
        _frameInfo = frameInfo;
        _image = image;
        _bounds = bounds;
        _position = new DarkAges.Library.Graphics.Vector2(bounds.X, bounds.Y);
        _isSelected = false;
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || spriteBatch == null) return;

        // Draw background
        spriteBatch.FillRectangle(_bounds, _isSelected ? Color.FromArgb(100, 255, 255, 0) : Color.FromArgb(50, 0, 0, 0));

        // Draw item image  
        if (_image != null && _frameInfo != null)
        {
            spriteBatch.Draw(_image, _frameInfo.SourceRect, _position, Color.White);
        }

        // Draw selection border if selected
        if (_isSelected)
        {
            spriteBatch.DrawRectangle(_bounds, Color.Yellow);
        }
    }
}
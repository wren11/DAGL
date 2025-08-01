using DarkAges.Library.Graphics;
using System.Drawing;
using System.Numerics;
using Vector2 = DarkAges.Library.Graphics.Vector2;

namespace DarkAges.Library.UI;

public class ImagePane(bool autoDisposeImage = true) : ControlPane
{
    public IndexedImage Image { get; set; }
    public Palette Palette { get; private set; }
    private IndexedImage _texture;

    public void SetImage(IndexedImage image, Palette palette)
    {
        if (_texture != null && autoDisposeImage)
        {
            _texture.Dispose();
            _texture = null;
        }
            
        Image = image;
        Palette = palette;

        if (Image != null && Palette != null)
        {
            var rgbaData = new byte[Image.Width * Image.Height * 4];
            for (var i = 0; i < Image.PixelData.Length; i++)
            {
                var paletteIndex = Image.PixelData[i];
                if (paletteIndex < Palette.Colors.Length)
                {
                    var color = Palette.Colors[paletteIndex];
                    rgbaData[i * 4 + 0] = color.R;
                    rgbaData[i * 4 + 1] = color.G;
                    rgbaData[i * 4 + 2] = color.B;
                    rgbaData[i * 4 + 3] = paletteIndex == 0 ? (byte)0 : (byte)255;
                }
            }
                
            _texture = new IndexedImage(Image.Width, Image.Height, rgbaData);
        }
    }
        
    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;
            
        if (_texture != null)
        {
            spriteBatch.Draw(_texture, new Vector2(Position.X, Position.Y), ColorRgb565.White);
        }
            
        base.Render(spriteBatch);
    }
}
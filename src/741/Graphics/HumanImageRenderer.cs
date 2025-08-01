using System.Drawing;
using DarkAges.Library.GameLogic;

namespace DarkAges.Library.Graphics;

public class HumanImageRenderer
{
    private readonly HumanImageCache _imageCache = new();
    private short _currentGender = 0;
    private short _currentAngle = 1;
    private short _currentHair = 1;
    private short _currentColor = 1;

    public void SetCharacter(short gender, short angle, short hair, short color)
    {
        _currentGender = gender;
        _currentAngle = angle;
        _currentHair = hair;
        _currentColor = color;
    }

    public void Render(SpriteBatch spriteBatch, int x, int y)
    {
        var baseImage = _imageCache.GetHumanImage(_currentGender, _currentAngle);
        if (baseImage == null)
            return;

        var hairImage = _imageCache.GetHairImage(_currentGender, _currentHair);
        var colorTable = ColoringTableManager.GetTable($"hair_{_currentColor}");

        if (hairImage != null)
        {
            var finalImage = ComposeCharacter(baseImage, hairImage, colorTable);
            if (finalImage != null)
            {
                spriteBatch.Draw(finalImage, new Vector2(x, y), ColorRgb565.White);
            }
        }
    }

    private IndexedImage ComposeCharacter(IndexedImage baseImage, IndexedImage hairImage, ColoringTable colorTable)
    {
        if (baseImage == null)
            return null;

        var composedData = new byte[baseImage.Width * baseImage.Height];
        System.Array.Copy(baseImage.PixelData, composedData, baseImage.PixelData.Length);

        if (hairImage != null && colorTable != null)
        {
            for (var i = 0; i < hairImage.PixelData.Length; i++)
            {
                if (hairImage.PixelData[i] != 0)
                {
                    var originalColor = hairImage.PixelData[i];
                    var newColor = colorTable.GetColor(originalColor);
                    composedData[i] = newColor;
                }
            }
        }

        return new IndexedImage(baseImage.Width, baseImage.Height, composedData);
    }

    public static IndexedImage Compose(User user, byte direction, short animationFrame, short emotionFrame)
    {
        var renderer = new HumanImageRenderer();
        renderer.SetCharacter(user.Gender, direction, user.HairStyle, user.HairColor);
            
        var baseImage = renderer._imageCache.GetHumanImage(user.Gender, direction);
        if (baseImage == null)
            return null;

        var hairImage = renderer._imageCache.GetHairImage(user.Gender, user.HairStyle);
        var colorTable = ColoringTableManager.GetTable($"hair_{user.HairColor}");

        return renderer.ComposeCharacter(baseImage, hairImage, colorTable);
    }
}
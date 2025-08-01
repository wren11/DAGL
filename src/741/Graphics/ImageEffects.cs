using System;

namespace DarkAges.Library.Graphics;

public static class ImageEffects
{
    public static ushort[] ToGrayscale(ushort[] pixelData)
    {
        var newPixelData = new ushort[pixelData.Length];
        for (var i = 0; i < pixelData.Length; i++)
        {
            var color = new ColorRgb565(pixelData[i]);
            var gray = (byte)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
            newPixelData[i] = new ColorRgb565(gray, gray, gray).Value;
        }
        return newPixelData;
    }
}
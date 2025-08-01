using System;
using System.Drawing;

namespace DarkAges.Library.Graphics;

public class Surface(int width, int height, byte[] pixelData) : IDisposable
{
    public int Width { get; private set; } = width;
    public int Height { get; private set; } = height;
    public byte[] PixelData { get; private set; } = pixelData;
    public bool IsDisposed { get; private set; }

    public Surface(int width, int height) : this(width, height, new byte[width * height * 4])
    {
    }

    public void SetPixel(int x, int y, Color color)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || IsDisposed)
            return;

        var index = (y * Width + x) * 4;
        PixelData[index] = color.R;
        PixelData[index + 1] = color.G;
        PixelData[index + 2] = color.B;
        PixelData[index + 3] = color.A;
    }

    public Color GetPixel(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || IsDisposed)
            return Color.Transparent;

        var index = (y * Width + x) * 4;
        return Color.FromArgb(
            PixelData[index + 3], // A
            PixelData[index],     // R
            PixelData[index + 1], // G
            PixelData[index + 2]  // B
        );
    }

    public void Clear(Color color)
    {
        if (IsDisposed) return;

        for (var i = 0; i < PixelData.Length; i += 4)
        {
            PixelData[i] = color.R;
            PixelData[i + 1] = color.G;
            PixelData[i + 2] = color.B;
            PixelData[i + 3] = color.A;
        }
    }

    public void Blit(Surface source, int destX, int destY)
    {
        if (IsDisposed || source.IsDisposed) return;

        for (var y = 0; y < source.Height; y++)
        {
            for (var x = 0; x < source.Width; x++)
            {
                var pixel = source.GetPixel(x, y);
                SetPixel(destX + x, destY + y, pixel);
            }
        }
    }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            PixelData = null!;
            IsDisposed = true;
        }
    }
}
using System;
using System.Drawing;
using Silk.NET.OpenGL;
using DarkAgesTexture = DarkAges.Library.Graphics.Texture;
using SystemBuffer = System.Buffer;

namespace DarkAges.Library.Graphics;

public class IndexedImage : IDisposable
{
    private byte[] _pixelData;
    private bool _isDisposed;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[] PixelData => _pixelData;
    public DarkAgesTexture Texture { get; private set; }
    public Palette Palette { get; set; }

    public IndexedImage(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        Width = width;
        Height = height;
        _pixelData = new byte[width * height];
        _isDisposed = false;
        Texture = new DarkAgesTexture(width, height, _pixelData);
        Palette = new Palette();
    }

    public IndexedImage(int width, int height, byte[] pixelData)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (pixelData == null) throw new ArgumentNullException(nameof(pixelData));
        if (pixelData.Length != width * height) throw new ArgumentException("Pixel data length does not match dimensions", nameof(pixelData));

        Width = width;
        Height = height;
        _pixelData = new byte[pixelData.Length];
        Array.Copy(pixelData, _pixelData, pixelData.Length);
        _isDisposed = false;
        Texture = new DarkAgesTexture(width, height, _pixelData);
        Palette = new Palette();
    }

    public void SetPixelData(byte[] data)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(IndexedImage));
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length != Width * Height) throw new ArgumentException("Data length does not match image dimensions", nameof(data));

        Array.Copy(data, _pixelData, data.Length);
        Texture.Update(_pixelData);
    }

    public void Clear()
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(IndexedImage));
        Array.Clear(_pixelData, 0, _pixelData.Length);
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _pixelData = null;
        Texture?.Dispose();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
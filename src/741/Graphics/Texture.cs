using System;
using System.Drawing;
using Silk.NET.OpenGL;

namespace DarkAges.Library.Graphics;

public class Texture : IDisposable
{
    private readonly GL? _gl;
    public uint Handle { get; }
    public uint TextureId => Handle;
    public int Width { get; }
    public int Height { get; }
    public bool IsDisposed { get; private set; }

    public Texture(GL gl, int width, int height, byte[] data)
    {
        _gl = gl;
        Width = width;
        Height = height;

        Handle = _gl.GenTexture();
        Bind();

        unsafe
        {
            fixed (void* d = data)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            }
        }

        SetParameters();
    }

    public Texture(GL gl, int width, int height)
    {
        _gl = gl;
        Width = width;
        Height = height;

        Handle = _gl.GenTexture();
        Bind();

        unsafe
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        }

        SetParameters();
    }

    public Texture(int imageWidth, int imageHeight, byte[] rgbaData)
    {
        _gl = null; // This constructor is for cases where GL context is not available
        Width = imageWidth;
        Height = imageHeight;
        Handle = 0; // No OpenGL texture handle
        if (rgbaData != null && rgbaData.Length > 0)
        {
            unsafe
            {
                fixed (void* d = rgbaData)
                {
                    _gl?.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)imageWidth, (uint)imageHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
                }
            }
        }
        IsDisposed = false;
    }

    private void SetParameters()
    {
        if (_gl == null) return;

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        if (_gl == null || IsDisposed) return;

        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void Update(byte[] data)
    {
        if (_gl == null || IsDisposed) return;

        Bind();
        unsafe
        {
            fixed (void* d = data)
            {
                _gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)Width, (uint)Height, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            }
        }
    }

    public void Dispose()
    {
        if (!IsDisposed && _gl != null)
        {
            _gl.DeleteTexture(Handle);
            IsDisposed = true;
        }
    }
}
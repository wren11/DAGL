using System.Numerics;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.Graphics;

/// <summary>
/// Represents an image layer in the world background
/// </summary>
public class ImageLayer(IndexedImage image) : IDisposable
{
    public IndexedImage Image { get; private set; } = image;
    public bool IsVisible { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;
    public float ScaleX { get; set; } = 1.0f;
    public float ScaleY { get; set; } = 1.0f;
    public float OffsetX { get; set; } = 0.0f;
    public float OffsetY { get; set; } = 0.0f;
    public float Rotation { get; set; } = 0.0f;
    public BlendMode BlendMode { get; set; } = BlendMode.Alpha;

    public void Render(SpriteBatch spriteBatch, Matrix4x4 parentTransform)
    {
        if (Image == null || !IsVisible)
            return;

        // Create texture if needed
        if (Image.Texture == null)
        {
            //Image.CreateTexture(spriteBatch.GL);
        }

        // Calculate layer transform
        var layerTransform = Matrix4x4.CreateScale(ScaleX, ScaleY, 1.0f) *
                Matrix4x4.CreateRotationZ(Rotation * (float)Math.PI / 180.0f) *
                Matrix4x4.CreateTranslation(OffsetX, OffsetY, 0.0f) *
                parentTransform;

        // Set blend mode and opacity
        spriteBatch.SetBlendMode(BlendMode);
        //spriteBatch.SetTransparency((int)(Opacity * 255));

        // Calculate destination rectangle
        var destRect = new System.Drawing.Rectangle(
            (int)OffsetX,
            (int)OffsetY,
            (int)(Image.Width * ScaleX),
            (int)(Image.Height * ScaleY)
        );

        // Render the layer
        spriteBatch.Draw(
            Image,
            destRect,
            new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height),
            System.Drawing.Color.White
        );
    }

    public void Dispose()
    {
        Image?.Dispose();
        Image = null;
    }
}
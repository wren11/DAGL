using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using DarkAges.Library.Core;
using DarkAges.Library.IO;
using DarkAges.Library.UI;

namespace DarkAges.Library.Graphics;

/// <summary>
/// Handles world background image rendering and management
/// </summary>
public class WorldBackImage : ControlPane, IDisposable
{
    private const int MAX_LAYERS = 8;
    private const int DEFAULT_WIDTH = 640;
    private const int DEFAULT_HEIGHT = 480;
    private const float DEFAULT_SCALE = 1.0f;

    // Image data
    private IndexedImage backgroundImage;
    private List<ImageLayer> layers;
    private bool isDisposed;
    private bool isVisible;
    private bool isEnabled;
    private bool isAnimated;

    // Rendering properties
    private float scaleX;
    private float scaleY;
    private float offsetX;
    private float offsetY;
    private float rotation;
    private ColorRgb555 backgroundColor;
    private bool useTransparency;
    private int transparencyLevel;

    // Animation properties
    private float animationSpeed;
    private float animationOffset;
    private DateTime lastAnimationTime;
    private bool isLooping;

    // Events
    public event Action<WorldBackImage> ImageLoaded;
    public event Action<WorldBackImage> ImageUnloaded;
    public event Action<WorldBackImage> AnimationStarted;
    public event Action<WorldBackImage> AnimationStopped;
    public event Action<WorldBackImage, WorldBackImageError> ImageError;

    public WorldBackImage() : this(DEFAULT_WIDTH, DEFAULT_HEIGHT)
    {
    }

    public WorldBackImage(int width, int height)
    {
        InitializeImage(width, height);
    }

    private void InitializeImage(int width, int height)
    {
        isDisposed = false;
        isVisible = true;
        isEnabled = true;
        isAnimated = false;

        // Initialize rendering properties
        scaleX = DEFAULT_SCALE;
        scaleY = DEFAULT_SCALE;
        offsetX = 0.0f;
        offsetY = 0.0f;
        rotation = 0.0f;
        backgroundColor = new ColorRgb555(0, 0, 0);
        useTransparency = false;
        transparencyLevel = 255;

        // Initialize animation properties
        animationSpeed = 0.0f;
        animationOffset = 0.0f;
        lastAnimationTime = DateTime.Now;
        isLooping = true;

        // Initialize collections
        layers = [];
        backgroundImage = null;

        // Set initial bounds
        SetBounds(new Rectangle(0, 0, width, height));
    }

    public void LoadImage(string imagePath)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(WorldBackImage));

        if (string.IsNullOrEmpty(imagePath))
            throw new ArgumentNullException(nameof(imagePath));

        try
        {
            // Unload current image
            UnloadImage();

            // Load new image
            backgroundImage = ImageLoader.LoadIndexedImage(imagePath);

            if (backgroundImage != null)
            {
                // Update bounds to match image size
                SetBounds(new Rectangle(0, 0, backgroundImage.Width, backgroundImage.Height));
                ImageLoaded?.Invoke(this);
            }
            else
            {
                throw new InvalidOperationException($"Failed to load image: {imagePath}");
            }
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.LoadFailed,
                Message = $"Failed to load image {imagePath}: {ex.Message}",
                Exception = ex
            });
            throw;
        }
    }

    public void LoadImageFromData(byte[] imageData, int width, int height)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(WorldBackImage));

        if (imageData == null)
            throw new ArgumentNullException(nameof(imageData));

        try
        {
            // Unload current image
            UnloadImage();

            // Create image from data
            backgroundImage = new IndexedImage(width, height);
            backgroundImage.SetPixelData(imageData);

            // Update bounds
            SetBounds(new Rectangle(0, 0, width, height));
            ImageLoaded?.Invoke(this);
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.LoadFailed,
                Message = $"Failed to load image from data: {ex.Message}",
                Exception = ex
            });
            throw;
        }
    }

    private void SetBounds(Rectangle rectangle)
    {
        Bounds = rectangle;
    }

    public void UnloadImage()
    {
        if (isDisposed)
            return;

        try
        {
            backgroundImage?.Dispose();
            backgroundImage = null;
            layers.Clear();
            ImageUnloaded?.Invoke(this);
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.UnloadFailed,
                Message = $"Failed to unload image: {ex.Message}",
                Exception = ex
            });
        }
    }

    public void AddLayer(ImageLayer layer)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(WorldBackImage));

        if (layer == null)
            throw new ArgumentNullException(nameof(layer));

        if (layers.Count >= MAX_LAYERS)
            throw new InvalidOperationException("Maximum number of layers reached");

        try
        {
            layers.Add(layer);
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.LayerAddFailed,
                Message = $"Failed to add layer: {ex.Message}",
                Exception = ex
            });
            throw;
        }
    }

    public void RemoveLayer(int layerIndex)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(WorldBackImage));

        if (layerIndex < 0 || layerIndex >= layers.Count)
            throw new ArgumentOutOfRangeException(nameof(layerIndex));

        try
        {
            var layer = layers[layerIndex];
            layers.RemoveAt(layerIndex);
            layer?.Dispose();
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.LayerRemoveFailed,
                Message = $"Failed to remove layer: {ex.Message}",
                Exception = ex
            });
            throw;
        }
    }

    public void ClearLayers()
    {
        if (isDisposed)
            return;

        try
        {
            foreach (var layer in layers)
            {
                layer?.Dispose();
            }
            layers.Clear();
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.LayerClearFailed,
                Message = $"Failed to clear layers: {ex.Message}",
                Exception = ex
            });
        }
    }

    public void SetScale(float scale)
    {
        SetScale(scale, scale);
    }

    public void SetScale(float scaleX, float scaleY)
    {
        if (isDisposed)
            return;

        this.scaleX = Math.Max(0.1f, scaleX);
        this.scaleY = Math.Max(0.1f, scaleY);
    }

    public void SetOffset(float offsetX, float offsetY)
    {
        if (isDisposed)
            return;

        this.offsetX = offsetX;
        this.offsetY = offsetY;
    }

    public void SetRotation(float rotation)
    {
        if (isDisposed)
            return;

        this.rotation = rotation % 360.0f;
    }

    public void SetBackgroundColor(ColorRgb555 color)
    {
        if (isDisposed)
            return;

        backgroundColor = color;
    }

    public void SetTransparency(bool useTransparency, int level = 255)
    {
        if (isDisposed)
            return;

        this.useTransparency = useTransparency;
        transparencyLevel = Math.Max(0, Math.Min(255, level));
    }

    public void StartAnimation(float speed, bool loop = true)
    {
        if (isDisposed)
            return;

        try
        {
            animationSpeed = speed;
            isLooping = loop;
            isAnimated = true;
            lastAnimationTime = DateTime.Now;
            AnimationStarted?.Invoke(this);
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.AnimationStartFailed,
                Message = $"Failed to start animation: {ex.Message}",
                Exception = ex
            });
        }
    }

    public void StopAnimation()
    {
        if (isDisposed)
            return;

        try
        {
            isAnimated = false;
            animationSpeed = 0.0f;
            AnimationStopped?.Invoke(this);
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.AnimationStopFailed,
                Message = $"Failed to stop animation: {ex.Message}",
                Exception = ex
            });
        }
    }

    public override void Update(float deltaTime)
    {
        if (isDisposed || !isEnabled || !isAnimated)
            return;

        try
        {
            var now = DateTime.Now;
            var elapsed = (float)(now - lastAnimationTime).TotalSeconds;
            lastAnimationTime = now;

            // Update animation offset
            animationOffset += animationSpeed * elapsed;

            // Handle looping
            if (isLooping && animationOffset > 1.0f)
            {
                animationOffset -= 1.0f;
            }
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.UpdateFailed,
                Message = $"Failed to update animation: {ex.Message}",
                Exception = ex
            });
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (isDisposed || !isVisible || backgroundImage == null)
            return;

        try
        {
            // Set up rendering state
            spriteBatch.SetBlendMode(useTransparency ? BlendMode.Alpha : BlendMode.Opaque);
            //spriteBatch.SetTransparency(transparencyLevel);

            // Calculate transformation matrix
            var transform = Matrix4x4.CreateScale(scaleX, scaleY, 1.0f) *
                    Matrix4x4.CreateRotationZ(rotation * (float)Math.PI / 180.0f) *
                    Matrix4x4.CreateTranslation(offsetX, offsetY, 0.0f);

            // Render background image
            RenderBackgroundImage(spriteBatch, transform);

            // Render layers
            RenderLayers(spriteBatch, transform);
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.RenderFailed,
                Message = $"Failed to render world background: {ex.Message}",
                Exception = ex
            });
        }
    }

    private void RenderBackgroundImage(SpriteBatch spriteBatch, Matrix4x4 transform)
    {
        if (backgroundImage == null)
            return;

        try
        {
            // Create texture if needed
            if (backgroundImage.Texture == null)
            {
                //backgroundImage.CreateTexture(spriteBatch.GL);
            }

            // Calculate destination rectangle
            var destRect = new System.Drawing.Rectangle(
                (int)(Bounds.X + offsetX),
                (int)(Bounds.Y + offsetY),
                (int)(backgroundImage.Width * scaleX),
                (int)(backgroundImage.Height * scaleY)
            );

            // Apply animation offset if animated
            if (isAnimated)
            {
                destRect.X += (int)(animationOffset * backgroundImage.Width);
            }

            // Render the image
            spriteBatch.Draw(
                backgroundImage,
                destRect,
                new Rectangle(0, 0, backgroundImage.Width, backgroundImage.Height),
                Color.White
            );
        }
        catch (Exception ex)
        {
            ImageError?.Invoke(this, new WorldBackImageError
            {
                ErrorCode = WorldBackImageErrorCode.RenderFailed,
                Message = $"Failed to render background image: {ex.Message}",
                Exception = ex
            });
        }
    }

    private void RenderLayers(SpriteBatch spriteBatch, Matrix4x4 transform)
    {
        foreach (var layer in layers)
        {
            if (layer != null && layer.IsVisible)
            {
                try
                {
                    layer.Render(spriteBatch, transform);
                }
                catch (Exception ex)
                {
                    ImageError?.Invoke(this, new WorldBackImageError
                    {
                        ErrorCode = WorldBackImageErrorCode.LayerRenderFailed,
                        Message = $"Failed to render layer: {ex.Message}",
                        Exception = ex
                    });
                }
            }
        }
    }

    public IndexedImage GetBackgroundImage()
    {
        return backgroundImage;
    }

    public List<ImageLayer> GetLayers()
    {
        return [..layers];
    }

    public WorldBackImageProperties GetProperties()
    {
        return new WorldBackImageProperties
        {
            ScaleX = scaleX,
            ScaleY = scaleY,
            OffsetX = offsetX,
            OffsetY = offsetY,
            Rotation = rotation,
            BackgroundColor = backgroundColor,
            UseTransparency = useTransparency,
            TransparencyLevel = transparencyLevel,
            IsAnimated = isAnimated,
            AnimationSpeed = animationSpeed,
            AnimationOffset = animationOffset,
            IsLooping = isLooping,
            LayerCount = layers.Count
        };
    }

    public new bool IsVisible()
    {
        return isVisible && !isDisposed;
    }

    public new bool IsEnabled()
    {
        return isEnabled && !isDisposed;
    }

    public new void SetVisible(bool visible)
    {
        isVisible = visible;
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected new virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // Dispose background image
                backgroundImage?.Dispose();
                backgroundImage = null;

                // Dispose layers
                foreach (var layer in layers)
                {
                    layer?.Dispose();
                }
                layers?.Clear();
            }

            isDisposed = true;
        }
    }
}
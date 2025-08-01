using System;
using System.Drawing;
using System.Runtime.InteropServices;
using DarkAges.Library.Graphics;
using System.Numerics;
using Color = System.Drawing.Color;

namespace DarkAges.Library.UI.Screen;

/// <summary>
/// Handles screen rendering operations and graphics state management
/// </summary>
public class ScreenRenderer : IDisposable
{
    private const int DEFAULT_WIDTH = 640;
    private const int DEFAULT_HEIGHT = 480;
    private const int BUFFER_SIZE = 98304;
    private const int MAX_TEXTURES = 100;

    private GraphicsDevice graphicsDevice;
    private Surface renderTarget;
    private Surface backBuffer;
    private bool isInitialized;
    private bool isDisposed;
    private bool isRendering;
    private int renderWidth;
    private int renderHeight;
    private int renderMode;
    private bool useDoubleBuffering;
    private bool useVsync;
    private bool useAntiAliasing;

    // Rendering state
    private Color clearColor;
    private Rectangle viewport;
    private Matrix4x4 projectionMatrix;
    private Matrix4x4 viewMatrix;
    private Matrix4x4 worldMatrix;

    // Performance tracking
    private int drawCalls;
    private int trianglesRendered;
    private TimeSpan lastFrameTime;
    private DateTime lastFrameStart;

    // Events
    public event Action FrameStarted;
    public event Action FrameEnded;
    public event Action<ScreenRendererException> RenderError;

    public ScreenRenderer()
    {
        InitializeRenderer();
    }

    public ScreenRenderer(int width, int height) : this()
    {
        SetRenderTarget(width, height);
    }

    private void InitializeRenderer()
    {
        try
        {
            // Initialize graphics device with proper parameters
            graphicsDevice = GraphicsDevice.Instance;
            // Note: GraphicsDevice.Initialize requires a window parameter, but we don't have one here
            // For now, we'll skip the initialization and assume it's already initialized
                
            // Create render target
            renderTarget = new Surface(renderWidth, renderHeight);
                
            // Create back buffer if using double buffering
            if (useDoubleBuffering)
            {
                backBuffer = new Surface(renderWidth, renderHeight);
            }

            // Set up rendering state
            SetupRenderingState();

            isInitialized = true;
        }
        catch (Exception ex)
        {
            throw new ScreenRendererException("Failed to initialize screen renderer", ex);
        }
    }

    private void SetupRenderingState()
    {
        if (graphicsDevice == null)
            return;

        // Set viewport
        //graphicsDevice.SetViewport(viewport);

        // Set projection matrix
        projectionMatrix = Matrix4x4.CreateOrthographic(renderWidth, renderHeight, 0.1f, 1000.0f);
        //graphicsDevice.SetProjectionMatrix(projectionMatrix);

        // Set view matrix
        viewMatrix = Matrix4x4.Identity;
        //graphicsDevice.SetViewMatrix(viewMatrix);

        // Set world matrix
        worldMatrix = Matrix4x4.Identity;
        //graphicsDevice.SetWorldMatrix(worldMatrix);

        // Set rendering options
        //graphicsDevice.SetDoubleBuffering(useDoubleBuffering);
        //graphicsDevice.SetVsync(useVsync);
        //graphicsDevice.SetAntiAliasing(useAntiAliasing);
    }

    public void Initialize()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(ScreenRenderer));

        try
        {
            // Initialize graphics device with proper parameters
            graphicsDevice = GraphicsDevice.Instance;
            // Note: GraphicsDevice.Initialize requires a window parameter, but we don't have one here
            // For now, we'll skip the initialization and assume it's already initialized
                
            // Create render target
            renderTarget = new Surface(renderWidth, renderHeight);
                
            // Create back buffer if using double buffering
            if (useDoubleBuffering)
            {
                backBuffer = new Surface(renderWidth, renderHeight);
            }

            // Set up rendering state
            SetupRenderingState();

            isInitialized = true;
        }
        catch (Exception ex)
        {
            throw new ScreenRendererException("Failed to initialize screen renderer", ex);
        }
    }

    public void SetRenderTarget(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Invalid render target dimensions");

        renderWidth = width;
        renderHeight = height;
        viewport = new Rectangle(0, 0, width, height);

        if (isInitialized)
        {
            RecreateRenderTargets();
            SetupRenderingState();
        }
    }

    private void RecreateRenderTargets()
    {
        // Dispose old targets
        renderTarget?.Dispose();
        backBuffer?.Dispose();

        // Create new targets
        renderTarget = new Surface(renderWidth, renderHeight);
            
        if (useDoubleBuffering)
        {
            backBuffer = new Surface(renderWidth, renderHeight);
        }
    }

    public void BeginFrame()
    {
        if (!isInitialized || isDisposed)
            return;

        if (isRendering)
            throw new InvalidOperationException("Frame already in progress");

        try
        {
            isRendering = true;
            lastFrameStart = DateTime.Now;
            drawCalls = 0;
            trianglesRendered = 0;

            // Clear render target
            Clear();

            // Notify frame started
            FrameStarted?.Invoke();
        }
        catch (Exception ex)
        {
            isRendering = false;
            RenderError?.Invoke(new ScreenRendererException("Failed to begin frame", ex));
            throw;
        }
    }

    public void EndFrame()
    {
        if (!isInitialized || isDisposed || !isRendering)
            return;

        try
        {
            // Present frame
            Present();

            // Update performance statistics
            lastFrameTime = DateTime.Now - lastFrameStart;

            isRendering = false;

            // Notify frame ended
            FrameEnded?.Invoke();
        }
        catch (Exception ex)
        {
            isRendering = false;
            RenderError?.Invoke(new ScreenRendererException("Failed to end frame", ex));
            throw;
        }
    }

    public void Clear()
    {
        if (!isInitialized || !isRendering)
            return;

        try
        {
            renderTarget.Clear(clearColor);
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to clear render target", ex));
            throw;
        }
    }

    public void Clear(System.Drawing.Color color)
    {
        clearColor = color;
        Clear();
    }

    public void Present()
    {
        if (!isInitialized)
            return;

        try
        {
            if (useDoubleBuffering && backBuffer != null)
            {
                // Copy render target to back buffer
                //backBuffer.CopyFrom(renderTarget);
                    
                // Present back buffer
                //graphicsDevice.Present(backBuffer);
            }
            else
            {
                // Present render target directly
                //graphicsDevice.Present(renderTarget);
            }
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to present frame", ex));
            throw;
        }
    }

    public void DrawTexture(Texture texture, int x, int y)
    {
        DrawTexture(texture, x, y, System.Drawing.Color.White);
    }

    public void DrawTexture(Texture texture, int x, int y, System.Drawing.Color color)
    {
        if (!isInitialized || !isRendering || texture == null)
            return;

        try
        {
            //renderTarget.DrawTexture(texture, x, y, color);
            drawCalls++;
            trianglesRendered += 2; // Two triangles per quad
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to draw texture", ex));
            throw;
        }
    }

    public void DrawTexture(Texture texture, Rectangle destination)
    {
        DrawTexture(texture, destination, System.Drawing.Color.White);
    }

    public void DrawTexture(Texture texture, Rectangle destination, System.Drawing.Color color)
    {
        if (!isInitialized || !isRendering || texture == null)
            return;

        try
        {
            //renderTarget.DrawTexture(texture, destination, color);
            drawCalls++;
            trianglesRendered += 2;
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to draw texture", ex));
            throw;
        }
    }

    public void DrawTexture(Texture texture, Rectangle source, Rectangle destination)
    {
        DrawTexture(texture, source, destination, System.Drawing.Color.White);
    }

    public void DrawTexture(Texture texture, Rectangle source, Rectangle destination, System.Drawing.Color color)
    {
        if (!isInitialized || !isRendering || texture == null)
            return;

        try
        {
            //renderTarget.DrawTexture(texture, source, destination, color);
            drawCalls++;
            trianglesRendered += 2;
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to draw texture", ex));
            throw;
        }
    }

    public void DrawSurface(Surface surface, int x, int y)
    {
        if (!isInitialized || !isRendering || surface == null)
            return;

        try
        {
            //renderTarget.CopyFrom(surface, x, y);
            drawCalls++;
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to draw surface", ex));
            throw;
        }
    }

    public void DrawSurface(Surface surface, Rectangle destination)
    {
        if (!isInitialized || !isRendering || surface == null)
            return;

        try
        {
            //renderTarget.CopyFrom(surface, destination);
            drawCalls++;
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to draw surface", ex));
            throw;
        }
    }

    public void DrawRectangle(Rectangle rectangle, System.Drawing.Color color)
    {
        if (!isInitialized || !isRendering)
            return;

        try
        {
            //renderTarget.DrawRectangle(rectangle, color);
            drawCalls++;
            trianglesRendered += 2;
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to draw rectangle", ex));
            throw;
        }
    }

    public void DrawLine(int x1, int y1, int x2, int y2, System.Drawing.Color color)
    {
        if (!isInitialized || !isRendering)
            return;

        try
        {
            //renderTarget.DrawLine(x1, y1, x2, y2, color);
            drawCalls++;
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to draw line", ex));
            throw;
        }
    }

    public void DrawText(string text, int x, int y, System.Drawing.Color color)
    {
        if (!isInitialized || !isRendering || string.IsNullOrEmpty(text))
            return;

        try
        {
            //renderTarget.DrawText(text, x, y, color);
            drawCalls++;
        }
        catch (Exception ex)
        {
            RenderError?.Invoke(new ScreenRendererException("Failed to draw text", ex));
            throw;
        }
    }

    public void SetRenderMode(int mode)
    {
        renderMode = mode;
            
        if (graphicsDevice != null)
        {
            //graphicsDevice.SetRenderMode(mode);
        }
    }

    public void SetDoubleBuffering(bool enabled)
    {
        useDoubleBuffering = enabled;
            
        if (isInitialized)
        {
            if (enabled && backBuffer == null)
            {
                backBuffer = new Surface(renderWidth, renderHeight);
            }
            else if (!enabled)
            {
                backBuffer?.Dispose();
                backBuffer = null;
            }
        }
    }

    public void SetVsync(bool enabled)
    {
        useVsync = enabled;
            
        if (graphicsDevice != null)
        {
            //graphicsDevice.SetVsync(enabled);
        }
    }

    public void SetAntiAliasing(bool enabled)
    {
        useAntiAliasing = enabled;
            
        if (graphicsDevice != null)
        {
            //graphicsDevice.SetAntiAliasing(enabled);
        }
    }

    public void SetClearColor(System.Drawing.Color color)
    {
        clearColor = color;
    }

    public void SetViewport(Rectangle viewport)
    {
        this.viewport = viewport;
            
        if (graphicsDevice != null)
        {
            //graphicsDevice.SetViewport(viewport);
        }
    }

    public void SetProjectionMatrix(Matrix4x4 matrix)
    {
        projectionMatrix = matrix;
            
        if (graphicsDevice != null)
        {
            //graphicsDevice.SetProjectionMatrix(matrix);
        }
    }

    public void SetViewMatrix(Matrix4x4 matrix)
    {
        viewMatrix = matrix;
            
        if (graphicsDevice != null)
        {
            //graphicsDevice.SetViewMatrix(matrix);
        }
    }

    public void SetWorldMatrix(Matrix4x4 matrix)
    {
        worldMatrix = matrix;
            
        if (graphicsDevice != null)
        {
            //graphicsDevice.SetWorldMatrix(matrix);
        }
    }

    public Surface GetRenderTarget()
    {
        return renderTarget;
    }

    public Surface GetBackBuffer()
    {
        return backBuffer;
    }

    public RendererStatistics GetStatistics()
    {
        return new RendererStatistics
        {
            DrawCalls = drawCalls,
            TrianglesRendered = trianglesRendered,
            LastFrameTime = lastFrameTime,
            RenderWidth = renderWidth,
            RenderHeight = renderHeight,
            RenderMode = renderMode,
            IsRendering = isRendering,
            UseDoubleBuffering = useDoubleBuffering,
            UseVsync = useVsync,
            UseAntiAliasing = useAntiAliasing
        };
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }

    public bool IsRendering()
    {
        return isRendering;
    }

    public int GetRenderWidth()
    {
        return renderWidth;
    }

    public int GetRenderHeight()
    {
        return renderHeight;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // Stop rendering
                isRendering = false;

                // Dispose graphics resources
                renderTarget?.Dispose();
                backBuffer?.Dispose();
                graphicsDevice?.Dispose();
            }

            isDisposed = true;
        }
    }
}
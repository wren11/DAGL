using System;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace DarkAges.Library.Graphics;

public class GraphicsDevice : IDisposable
{
    private GL? _gl;
    public GL GL => _gl ?? throw new InvalidOperationException("Graphics device not initialized");
    
    private static GraphicsDevice? _instance;
    public static GraphicsDevice Instance => _instance ??= new GraphicsDevice();

    private IWindow? _window;
    private uint _vertexBuffer;
    private uint _vertexArray;
    private uint _shaderProgram;
    private Matrix4x4 _projectionMatrix;
    private Matrix4x4 _viewMatrix;
    private bool _isInitialized;
    private bool _isDisposed;

    // Vertex data for rendering
    private readonly List<float> _vertices = new();
    private readonly List<uint> _indices = new();

    private SpriteBatch? _spriteBatch;

    public bool IsInitialized => _gl != null && _window != null && _isInitialized;
    public int Width => _window?.Size.X ?? 0;
    public int Height => _window?.Size.Y ?? 0;

    public GraphicsDevice()
    {
    }

    public void Initialize(IWindow window)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(GraphicsDevice));
            
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _gl = GL.GetApi(window);
            
        SetupOpenGL();
        CreateShaders();
        CreateBuffers();
        SetupMatrices();
            
        _spriteBatch = new SpriteBatch(_gl, Width, Height);
        _isInitialized = true;
    }

    private void SetupOpenGL()
    {
        if (_gl == null) return;

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthFunc(DepthFunction.Lequal);
    }

    private void CreateShaders()
    {
        if (_gl == null) return;

        // Vertex shader source
        const string vertexShaderSource = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 uProjection;
uniform mat4 uView;
uniform mat4 uModel;

out vec4 fColor;
out vec2 fTexCoord;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
    fColor = aColor;
    fTexCoord = aTexCoord;
}";

        // Fragment shader source
        const string fragmentShaderSource = @"
#version 330 core
in vec4 fColor;
in vec2 fTexCoord;

uniform sampler2D uTexture;
uniform bool uUseTexture;

out vec4 FragColor;

void main()
{
    if (uUseTexture) {
        FragColor = texture(uTexture, fTexCoord) * fColor;
    } else {
        FragColor = fColor;
    }
}";

        // Compile vertex shader
        var vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexShaderSource);
        _gl.CompileShader(vertexShader);
        CheckShaderCompilation(vertexShader, "VERTEX");

        // Compile fragment shader
        var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentShaderSource);
        _gl.CompileShader(fragmentShader);
        CheckShaderCompilation(fragmentShader, "FRAGMENT");

        // Create shader program
        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, vertexShader);
        _gl.AttachShader(_shaderProgram, fragmentShader);
        _gl.LinkProgram(_shaderProgram);
        CheckProgramLinking(_shaderProgram);

        // Cleanup shaders
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    private void CheckShaderCompilation(uint shader, string type)
    {
        if (_gl == null) return;

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var success);
        if (success == 0)
        {
            var infoLog = _gl.GetShaderInfoLog(shader);
            throw new Exception($"Shader compilation failed ({type}): {infoLog}");
        }
    }

    private void CheckProgramLinking(uint program)
    {
        if (_gl == null) return;

        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out var success);
        if (success == 0)
        {
            var infoLog = _gl.GetProgramInfoLog(program);
            throw new Exception($"Shader program linking failed: {infoLog}");
        }
    }

    private void CreateBuffers()
    {
        if (_gl == null) return;

        // Create vertex array object
        _vertexArray = _gl.GenVertexArray();
        _gl.BindVertexArray(_vertexArray);

        // Create vertex buffer object
        _vertexBuffer = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);

        // Define vertex attributes
        // Position (3 floats)
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);

        // Color (4 floats)
        _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
        _gl.EnableVertexAttribArray(1);

        // Texture coordinates (2 floats)
        _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 7 * sizeof(float));
        _gl.EnableVertexAttribArray(2);

        _gl.BindVertexArray(0);
    }

    private void SetupMatrices()
    {
        _projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, Width, Height, 0, -1, 1);
        _viewMatrix = Matrix4x4.Identity;
    }

    public void BeginFrame()
    {
        if (_gl == null) return;

        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.UseProgram(_shaderProgram);

        // Set matrices
        SetMatrix4("uProjection", _projectionMatrix);
        SetMatrix4("uView", _viewMatrix);

        _vertices.Clear();
        _indices.Clear();
    }

    public void EndFrame()
    {
        if (_vertices.Count > 0)
        {
            FlushBatch();
        }
    }

    private void FlushBatch()
    {
        if (_gl == null || _vertices.Count == 0) return;

        _gl.BindVertexArray(_vertexArray);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);

        unsafe
        {
            fixed (float* vertexPtr = _vertices.ToArray())
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_vertices.Count * sizeof(float)), vertexPtr, BufferUsageARB.DynamicDraw);
            }
        }

        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)(_vertices.Count / 9));
        _gl.BindVertexArray(0);

        _vertices.Clear();
        _indices.Clear();
    }

    public void DrawImage(Image image, int x, int y)
    {
        if (image == null) return;
            
        var rect = new Rectangle(x, y, image.Width, image.Height);
        FillRectangle(rect, Color.White);
    }

    public void DrawImage(IndexedImage image, int x, int y, Color tint)
    {
        if (image == null) return;

        var rect = new Rectangle(x, y, image.Width, image.Height);
        FillRectangle(rect, tint);
    }

    public void DrawImage(IndexedImage image, Rectangle destRect, Rectangle sourceRect)
    {
        if (image == null) return;
        _spriteBatch?.Draw(image, destRect, sourceRect, Color.White);
    }

    public void DrawImage(IndexedImage image, Rectangle destRect, Rectangle sourceRect, Color tint)
    {
        if (image == null) return;
        _spriteBatch?.Draw(image, destRect, sourceRect, tint);
    }

    public void DrawRectangle(Rectangle rect, Color color, int borderWidth = 1)
    {
        if (borderWidth <= 0) return;

        // Draw four lines for the rectangle border
        DrawLine(rect.X, rect.Y, rect.X + rect.Width, rect.Y, color, borderWidth); // Top
        DrawLine(rect.X, rect.Y, rect.X, rect.Y + rect.Height, color, borderWidth); // Left
        DrawLine(rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + rect.Height, color, borderWidth); // Right
        DrawLine(rect.X, rect.Y + rect.Height, rect.X + rect.Width, rect.Y + rect.Height, color, borderWidth); // Bottom
    }

    public void FillRectangle(Rectangle rect, Color color)
    {
        FillRectangle(rect.X, rect.Y, rect.Width, rect.Height, color);
    }

    public void FillRectangle(int x, int y, int width, int height, Color color)
    {
        var normalizedColor = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

        // Add vertices for two triangles forming a rectangle
        AddVertex(x, y, 0, normalizedColor, 0, 0);
        AddVertex(x + width, y, 0, normalizedColor, 1, 0);
        AddVertex(x, y + height, 0, normalizedColor, 0, 1);

        AddVertex(x + width, y, 0, normalizedColor, 1, 0);
        AddVertex(x + width, y + height, 0, normalizedColor, 1, 1);
        AddVertex(x, y + height, 0, normalizedColor, 0, 1);

        SetBool("uUseTexture", false);
    }

    public void DrawText(string text, int x, int y, Color color, object? font)
    {
        if (font is SimpleFont simpleFont)
        {
            simpleFont.DrawString(_spriteBatch, text, new Vector2(x, y), color);
        }
    }

    private void DrawBitmapText(string text, int x, int y, Color color)
    {
        const int charWidth = 8;
        const int charHeight = 12;
            
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            var charX = x + (i * charWidth);
                
            // Draw character bitmap
            DrawCharacterBitmap(c, charX, y, charWidth, charHeight, color);
        }
    }

    private void DrawCharacterBitmap(char c, int x, int y, int width, int height, Color color)
    {
        // Simple bitmap font patterns for common characters
        var pattern = GetCharacterPattern(c);
            
        for (var row = 0; row < 8; row++)
        {
            var rowPattern = pattern[row];
            for (var col = 0; col < 8; col++)
            {
                if ((rowPattern & (1 << (7 - col))) != 0)
                {
                    // Draw pixel
                    FillRectangle(x + col, y + row, 1, 1, color);
                }
            }
        }
    }

    private byte[] GetCharacterPattern(char c)
    {
        // Basic 8x8 bitmap patterns for common characters
        return c switch
        {
            'A' => [0x18, 0x3C, 0x66, 0x66, 0x7E, 0x66, 0x66, 0x00],
            'B' => [0x7C, 0x66, 0x66, 0x7C, 0x66, 0x66, 0x7C, 0x00],
            'C' => [0x3C, 0x66, 0x60, 0x60, 0x60, 0x66, 0x3C, 0x00],
            'D' => [0x78, 0x6C, 0x66, 0x66, 0x66, 0x6C, 0x78, 0x00],
            'E' => [0x7E, 0x60, 0x60, 0x78, 0x60, 0x60, 0x7E, 0x00],
            '0' => [0x3C, 0x66, 0x6E, 0x76, 0x66, 0x66, 0x3C, 0x00],
            '1' => [0x18, 0x38, 0x18, 0x18, 0x18, 0x18, 0x7E, 0x00],
            '2' => [0x3C, 0x66, 0x06, 0x0C, 0x18, 0x30, 0x7E, 0x00],
            '3' => [0x3C, 0x66, 0x06, 0x1C, 0x06, 0x66, 0x3C, 0x00],
            ' ' => [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00],
            '.' => [0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x18, 0x00],
            ',' => [0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x30, 0x00],
            ':' => [0x00, 0x00, 0x18, 0x00, 0x00, 0x18, 0x00, 0x00],
            '!' => [0x18, 0x18, 0x18, 0x18, 0x00, 0x18, 0x18, 0x00],
            '?' => [0x3C, 0x66, 0x06, 0x0C, 0x18, 0x00, 0x18, 0x00],
            _ => [0x7E, 0x42, 0x42, 0x42, 0x42, 0x42, 0x7E, 0x00] // Default rectangle
        };
    }

    public void DrawTexture(Texture texture, float x, float y, Rectangle sourceRect, Color tint)
    {
        if (texture?.TextureId == null) return;

        var destRect = new RectangleF(x, y, sourceRect.Width, sourceRect.Height);
        DrawTexture(texture, destRect, sourceRect, Color.White);
    }

    public void DrawTexture(Texture texture, RectangleF destRect, Rectangle sourceRect, Color tint)
    {
        if (texture?.TextureId == null) return;

        _gl?.BindTexture(TextureTarget.Texture2D, texture.TextureId);
        SetBool("uUseTexture", true);

        var normalizedColor = new Vector4(tint.R / 255.0f, tint.G / 255.0f, tint.B / 255.0f, tint.A / 255.0f);

        // Calculate texture coordinates
        var texLeft = sourceRect.X / (float)texture.Width;
        var texTop = sourceRect.Y / (float)texture.Height;
        var texRight = (sourceRect.X + sourceRect.Width) / (float)texture.Width;
        var texBottom = (sourceRect.Y + sourceRect.Height) / (float)texture.Height;

        // Add vertices for textured rectangle
        AddVertex(destRect.X, destRect.Y, 0, normalizedColor, texLeft, texTop);
        AddVertex(destRect.X + destRect.Width, destRect.Y, 0, normalizedColor, texRight, texTop);
        AddVertex(destRect.X, destRect.Y + destRect.Height, 0, normalizedColor, texLeft, texBottom);

        AddVertex(destRect.X + destRect.Width, destRect.Y, 0, normalizedColor, texRight, texTop);
        AddVertex(destRect.X + destRect.Width, destRect.Y + destRect.Height, 0, normalizedColor, texRight, texBottom);
        AddVertex(destRect.X, destRect.Y + destRect.Height, 0, normalizedColor, texLeft, texBottom);
    }

    public void DrawLine(int x1, int y1, int x2, int y2, Color color, int width = 1)
    {
        // For thick lines, draw as rectangles
        if (width > 1)
        {
            DrawThickLine(x1, y1, x2, y2, color, width);
            return;
        }

        var normalizedColor = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

        // Draw line as two triangles forming a thin rectangle
        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = (float)Math.Sqrt(dx * dx + dy * dy);
            
        if (length < 0.001f) return;

        var normalX = -dy / length;
        var normalY = dx / length;
        var halfWidth = 0.5f;

        var x1f = (float)x1;
        var y1f = (float)y1;
        var x2f = (float)x2;
        var y2f = (float)y2;

        AddVertex(x1f + normalX * halfWidth, y1f + normalY * halfWidth, 0, normalizedColor, 0, 0);
        AddVertex(x1f - normalX * halfWidth, y1f - normalY * halfWidth, 0, normalizedColor, 0, 1);
        AddVertex(x2f + normalX * halfWidth, y2f + normalY * halfWidth, 0, normalizedColor, 1, 0);

        AddVertex(x1f - normalX * halfWidth, y1f - normalY * halfWidth, 0, normalizedColor, 0, 1);
        AddVertex(x2f - normalX * halfWidth, y2f - normalY * halfWidth, 0, normalizedColor, 1, 1);
        AddVertex(x2f + normalX * halfWidth, y2f + normalY * halfWidth, 0, normalizedColor, 1, 0);

        SetBool("uUseTexture", false);
    }

    private void DrawThickLine(int x1, int y1, int x2, int y2, Color color, int width)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = Math.Sqrt(dx * dx + dy * dy);

        if (length < 0.001f) return;

        var normalX = (float)(-dy / length) * (width / 2.0f);
        var normalY = (float)(dx / length) * (width / 2.0f);

        var normalizedColor = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

        AddVertex(x1 + normalX, y1 + normalY, 0, normalizedColor, 0, 0);
        AddVertex(x1 - normalX, y1 - normalY, 0, normalizedColor, 0, 1);
        AddVertex(x2 + normalX, y2 + normalY, 0, normalizedColor, 1, 0);

        AddVertex(x1 - normalX, y1 - normalY, 0, normalizedColor, 0, 1);
        AddVertex(x2 - normalX, y2 - normalY, 0, normalizedColor, 1, 1);
        AddVertex(x2 + normalX, y2 + normalY, 0, normalizedColor, 1, 0);

        SetBool("uUseTexture", false);
    }

    private void AddVertex(float x, float y, float z, Vector4 color, float u, float v)
    {
        _vertices.AddRange([x, y, z, color.X, color.Y, color.Z, color.W, u, v]);
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        _gl?.Viewport(x, y, (uint)width, (uint)height);
    }

    public void Resize(int width, int height)
    {
        if (_gl == null) return;

        _gl.Viewport(0, 0, (uint)width, (uint)height);
        _projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1.0f, 1.0f);
    }

    private void SetMatrix4(string name, Matrix4x4 matrix)
    {
        if (_gl == null) return;

        var location = _gl.GetUniformLocation(_shaderProgram, name);
        if (location != -1)
        {
            unsafe
            {
                _gl.UniformMatrix4(location, 1, false, (float*)&matrix);
            }
        }
    }

    private void SetBool(string name, bool value)
    {
        if (_gl == null) return;

        var location = _gl.GetUniformLocation(_shaderProgram, name);
        if (location != -1)
        {
            _gl.Uniform1(location, value ? 1 : 0);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        _spriteBatch?.Dispose();
        _gl?.DeleteBuffer(_vertexBuffer);
        _gl?.DeleteVertexArray(_vertexArray);
        _gl?.DeleteProgram(_shaderProgram);
        
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    public void DrawTexture(Texture currentFrameTexture, RectangleF destRect, Rectangle currentFrameSourceRect)
    {
        if (currentFrameTexture == null || currentFrameTexture.TextureId == 0) return;
        _gl?.BindTexture(TextureTarget.Texture2D, currentFrameTexture.TextureId);
        SetBool("uUseTexture", true);
        var normalizedColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // White color for texture
        // Calculate texture coordinates
        var texLeft = currentFrameSourceRect.X / (float)currentFrameTexture.Width;
        var texTop = currentFrameSourceRect.Y / (float)currentFrameTexture.Height;
        var texRight = (currentFrameSourceRect.X + currentFrameSourceRect.Width) / (float)currentFrameTexture.Width;
        var texBottom = (currentFrameSourceRect.Y + currentFrameSourceRect.Height) / (float)currentFrameTexture.Height;
        // Add vertices for textured rectangle
        AddVertex(destRect.X, destRect.Y, 0, normalizedColor, texLeft, texTop);
        AddVertex(destRect.X + destRect.Width, destRect.Y, 0, normalizedColor, texRight, texTop);
        AddVertex(destRect.X, destRect.Y + destRect.Height, 0, normalizedColor, texLeft, texBottom);
        AddVertex(destRect.X + destRect.Width, destRect.Y, 0, normalizedColor, texRight, texTop);
        AddVertex(destRect.X + destRect.Width, destRect.Y + destRect.Height, 0, normalizedColor, texRight, texBottom);
        AddVertex(destRect.X, destRect.Y + destRect.Height, 0, normalizedColor, texLeft, texBottom);

    }

    public void DrawTexture(Texture currentFrameTexture, float f, float f1, Rectangle currentFrameSourceRect)
    {
        if (currentFrameTexture == null || currentFrameTexture.TextureId == 0) return;
        _gl?.BindTexture(TextureTarget.Texture2D, currentFrameTexture.TextureId);
        SetBool("uUseTexture", true);
        var normalizedColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // White color for texture
        // Calculate texture coordinates
        var texLeft = currentFrameSourceRect.X / (float)currentFrameTexture.Width;
        var texTop = currentFrameSourceRect.Y / (float)currentFrameTexture.Height;
        var texRight = (currentFrameSourceRect.X + currentFrameSourceRect.Width) / (float)currentFrameTexture.Width;
        var texBottom = (currentFrameSourceRect.Y + currentFrameSourceRect.Height) / (float)currentFrameTexture.Height;
        // Add vertices for textured rectangle
        AddVertex(f, f1, 0, normalizedColor, texLeft, texTop);
        AddVertex(f + currentFrameSourceRect.Width, f1, 0, normalizedColor, texRight, texTop);
        AddVertex(f, f1 + currentFrameSourceRect.Height, 0, normalizedColor, texLeft, texBottom);
        AddVertex(f + currentFrameSourceRect.Width, f1, 0, normalizedColor, texRight, texTop);
        AddVertex(f + currentFrameSourceRect.Width, f1 + currentFrameSourceRect.Height, 0, normalizedColor, texRight, texBottom);
        AddVertex(f, f1 + currentFrameSourceRect.Height, 0, normalizedColor, texLeft, texBottom);
    }
}
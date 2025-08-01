using Silk.NET.OpenGL;
using System.Drawing;
using System.Drawing.Drawing2D;
using Silk.NET.Maths;
using Rectangle = System.Drawing.Rectangle;

namespace DarkAges.Library.Graphics;

public class SpriteBatch : IDisposable
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly BufferObject<float> _vbo;
    private readonly BufferObject<uint> _ebo;
    private readonly VertexArrayObject<float, uint> _vao;
    private readonly Texture _texture;
    private readonly List<Vertex> _vertices = [];
    private readonly List<uint> _indices = [];
    private Matrix4X4<int> _projection;
    private bool _isDisposed;

    public SpriteBatch(GL gl, int width, int height)
    {
        _gl = gl;

        _shader = new Shader(_gl, 
            @"#version 330 core
                layout (location = 0) in vec2 aPosition;
                layout (location = 1) in vec2 aTexCoords;
                layout (location = 2) in vec4 aColor;
                out vec2 TexCoords;
                out vec4 Color;
                uniform mat4 projection;
                void main()
                {
                    TexCoords = aTexCoords;
                    Color = aColor;
                    gl_Position = projection * vec4(aPosition, 0.0, 1.0);
                }",
            @"#version 330 core
                out vec4 FragColor;
                in vec2 TexCoords;
                in vec4 Color;
                uniform sampler2D ourTexture;
                void main()
                {
                    FragColor = Color * texture(ourTexture, TexCoords);
                }");

        _vbo = new BufferObject<float>(_gl, _vertices.ToArray(), BufferTargetARB.ArrayBuffer);
        _ebo = new BufferObject<uint>(_gl, _indices.ToArray(), BufferTargetARB.ElementArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

        _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 8, 0);
        _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 8, 2);
        _vao.VertexAttributePointer(2, 4, VertexAttribPointerType.Float, 8, 4);

        _texture = new Texture(_gl, "default.png");
        UpdateProjection(width, height);
    }

    public void Clear(Color color)
    {
        _gl.ClearColor(color);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void Begin()
    {
        _vertices.Clear();
        _indices.Clear();
    }

    public void End()
    {
        _vbo.Bind();
        _vbo.BufferData(_vertices.ToArray());
        _ebo.Bind();
        _ebo.BufferData(_indices.ToArray());

        _shader.Use();
        _shader.SetUniform("projection", _projection);
        _texture.Bind();
        _vao.Bind();

        unsafe
        {
            _gl.DrawElements(PrimitiveType.Triangles, (uint)_indices.Count, DrawElementsType.UnsignedInt, null);
        }
    }

    public void Draw(IndexedImage texture, Rectangle destRect, Vector2 position, Color color)
    {
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(position, new Vector2(0, 0), color));
        _vertices.Add(new Vertex(new Vector2(position.X + texture.Width, position.Y), new Vector2(1, 0), color));
        _vertices.Add(new Vertex(new Vector2(position.X + texture.Width, position.Y + texture.Height), new Vector2(1, 1), color));
        _vertices.Add(new Vertex(new Vector2(position.X, position.Y + texture.Height), new Vector2(0, 1), color));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void DrawString(object font, string text, Vector2 position, Color color)
    {
        if (font is SimpleFont simpleFont)
        {
            simpleFont.DrawString(this, text, position, color);
        }
    }

    public void DrawString(SimpleFont font, string text, DarkAges.Library.Graphics.Vector2 position, Color color)
    {
        font.DrawString(this, text, position, color);
    }

    public void FillRectangle(Rectangle rectangle, Color color)
    {
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(new Vector2(rectangle.X, rectangle.Y), Vector2.Zero, color));
        _vertices.Add(new Vertex(new Vector2(rectangle.Right, rectangle.Y), Vector2.Zero, color));
        _vertices.Add(new Vertex(new Vector2(rectangle.Right, rectangle.Bottom), Vector2.Zero, color));
        _vertices.Add(new Vertex(new Vector2(rectangle.X, rectangle.Bottom), Vector2.Zero, color));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void DrawRectangle(Rectangle rectangle, System.Drawing.Color color)
    {
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(new Vector2(rectangle.X, rectangle.Y), Vector2.Zero, color));
        _vertices.Add(new Vertex(new Vector2(rectangle.Right, rectangle.Y), Vector2.Zero, color));
        _vertices.Add(new Vertex(new Vector2(rectangle.Right, rectangle.Bottom), Vector2.Zero, color));
        _vertices.Add(new Vertex(new Vector2(rectangle.X, rectangle.Bottom), Vector2.Zero, color));
        _indices.AddRange([index, index + 1, index + 1, index + 2, index + 2, index + 3, index + 3, index]);
    }

    public void DrawLine(DarkAges.Library.Graphics.Vector2 start, DarkAges.Library.Graphics.Vector2 end, Color color)
    {
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(start, Vector2.Zero, color));
        _vertices.Add(new Vertex(end, Vector2.Zero, color));
        _indices.AddRange([index, index + 1]);
    }

    public void UpdateProjection(int width, int height)
    {
        _projection = Matrix4X4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
        _shader.Dispose();
        _texture.Dispose();
        _isDisposed = true;
    }


    public void DrawCircle(Vector2 position, float radius, Color color)
    {
        const int segments = 32;
        var angleIncrement = MathF.PI * 2 / segments;
        var index = (uint)_vertices.Count;
        for (var i = 0; i < segments; i++)
        {
            var angle = i * angleIncrement;
            var x = position.X + MathF.Cos(angle) * radius;
            var y = position.Y + MathF.Sin(angle) * radius;
            _vertices.Add(new Vertex(new Vector2(x, y), Vector2.Zero, color));
        }
        for (var i = 0; i < segments; i++)
        {
            _indices.Add(index + (uint)i);
            _indices.Add(index + (uint)((i + 1) % segments));
        }
    }

    public void DrawString(object? font, string displayName, float namePositionX, float namePositionY, Color black)
    {
        if (font is SimpleFont simpleFont)
        {
            simpleFont.DrawString(this, displayName, new Vector2(namePositionX, namePositionY), black);
        }
        else
        {
            throw new ArgumentException("Invalid font type. Expected SimpleFont.");
        }
    }

    public void DrawString(SimpleFont font, string displayName, Point namePositionX, Color namePositionY)
    {
        if (font != null)
        {
            font.DrawString(this, displayName, new Vector2(namePositionX.X, namePositionX.Y), namePositionY);
        }
        else
        {
            throw new ArgumentNullException(nameof(font), "Font cannot be null.");
        }
    }

    public void DrawTexture(Texture fontTexture, Rectangle charRect, Rectangle sourceRect, Color color)
    {
        if (fontTexture == null)
        {
            throw new ArgumentNullException(nameof(fontTexture), "Texture cannot be null.");
        }
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(new Vector2(charRect.X, charRect.Y), new Vector2(sourceRect.X / (float)fontTexture.Width, sourceRect.Y / (float)fontTexture.Height), color));
        _vertices.Add(new Vertex(new Vector2(charRect.Right, charRect.Y), new Vector2((sourceRect.X + sourceRect.Width) / (float)fontTexture.Width, sourceRect.Y / (float)fontTexture.Height), color));
        _vertices.Add(new Vertex(new Vector2(charRect.Right, charRect.Bottom), new Vector2((sourceRect.X + sourceRect.Width) / (float)fontTexture.Width, (sourceRect.Y + sourceRect.Height) / (float)fontTexture.Height), color));
        _vertices.Add(new Vertex(new Vector2(charRect.X, charRect.Bottom), new Vector2(sourceRect.X / (float)fontTexture.Width, (sourceRect.Y + sourceRect.Height) / (float)fontTexture.Height), color));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void DrawRectangle(Rectangle rectangle, ColorRgb565 white, bool b)
    {
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(new Vector2(rectangle.X, rectangle.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(rectangle.Right, rectangle.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(rectangle.Right, rectangle.Bottom), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(rectangle.X, rectangle.Bottom), Vector2.Zero, white));
        _indices.AddRange([index, index + 1, index + 1, index + 2, index + 2, index + 3, index + 3, index]);
    }

    public void Draw(IndexedImage finalImage, Rectangle position, Color color, ColorRgb565 white)
    {
        if (finalImage == null)
        {
            throw new ArgumentNullException(nameof(finalImage), "IndexedImage cannot be null.");
        }
        var index = (uint)_vertices.Count;
        for (var y = 0; y < finalImage.Height; y++)
        {
            for (var x = 0; x < finalImage.Width; x++)
            {
                var pixelIndex = y * finalImage.Width + x;
                var paletteIndex = finalImage.PixelData[pixelIndex];
                if (paletteIndex > 0) // Only draw non-transparent pixels (palette index 0 is typically transparent)
                {
                    var pixelColor = finalImage.Palette.GetColor(paletteIndex);
                    _vertices.Add(new Vertex(new Vector2(position.X + x, position.Y + y), Vector2.Zero, pixelColor));
                }
            }
        }
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void SetBlendMode(BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Alpha:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                break;
            case BlendMode.Add:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                break;
            case BlendMode.Overlay:
                _gl.Disable(EnableCap.Blend);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(blendMode), blendMode, null);
        }
    }

    public void Draw(IndexedImage? backgroundImageTexture, Rectangle destRect, Rectangle color, Color white)
    {
        if (backgroundImageTexture == null)
        {
            throw new ArgumentNullException(nameof(backgroundImageTexture), "Background image cannot be null.");
        }
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.Right, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.Right, destRect.Bottom), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Bottom), Vector2.Zero, white));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void Draw(Texture backgroundImageTexture, Vector2 destRect, Color color, ColorRgb565 white)
    {
        if (backgroundImageTexture == null)
        {
            throw new ArgumentNullException(nameof(backgroundImageTexture), "Background image cannot be null.");
        }
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(destRect, Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X + backgroundImageTexture.Width, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X + backgroundImageTexture.Width, destRect.Y + backgroundImageTexture.Height), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Y + backgroundImageTexture.Height), Vector2.Zero, white));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void Draw(Image backgroundImageTexture, Rectangle destRect, Color color, ColorRgb565 white)
    {
        if (backgroundImageTexture == null)
        {
            throw new ArgumentNullException(nameof(backgroundImageTexture), "Background image cannot be null.");
        }
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.Right, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.Right, destRect.Bottom), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Bottom), Vector2.Zero, white));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void Draw(IndexedImage backgroundImageTexture, Rectangle destRect, Color color, Color white)
    {
        if (backgroundImageTexture == null)
        {
            throw new ArgumentNullException(nameof(backgroundImageTexture), "Background image cannot be null.");
        }
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.Right, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.Right, destRect.Bottom), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Bottom), Vector2.Zero, white));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void Draw(IndexedImage backgroundImageTexture, System.Numerics.Vector2 destRect, Color color, ColorRgb565 white)
    {
        if (backgroundImageTexture == null)
        {
            throw new ArgumentNullException(nameof(backgroundImageTexture), "Background image cannot be null.");
        }
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X + backgroundImageTexture.Width, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X + backgroundImageTexture.Width, destRect.Y + backgroundImageTexture.Height), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Y + backgroundImageTexture.Height), Vector2.Zero, white));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void Draw(IndexedImage backgroundImageTexture, Vector2 destRect, Color color, ColorRgb565 white)
    {
        if (backgroundImageTexture == null)
        {
            throw new ArgumentNullException(nameof(backgroundImageTexture), "Background image cannot be null.");
        }
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(destRect, Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X + backgroundImageTexture.Width, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X + backgroundImageTexture.Width, destRect.Y + backgroundImageTexture.Height), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Y + backgroundImageTexture.Height), Vector2.Zero, white));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void Draw(IndexedImage backgroundImageTexture, Vector2 destRect, ColorRgb565 white)
    {
        if (backgroundImageTexture == null)
        {
            throw new ArgumentNullException(nameof(backgroundImageTexture), "Background image cannot be null.");
        }
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(destRect, Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X + backgroundImageTexture.Width, destRect.Y), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X + backgroundImageTexture.Width, destRect.Y + backgroundImageTexture.Height), Vector2.Zero, white));
        _vertices.Add(new Vertex(new Vector2(destRect.X, destRect.Y + backgroundImageTexture.Height), Vector2.Zero, white));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void DrawImage(IndexedImage itemIcon, int boundsX, int itemHeight, int iconSize, int i)
    {
        if (itemIcon == null)
        {
            throw new ArgumentNullException(nameof(itemIcon), "Item icon cannot be null.");
        }
        var index = (uint)_vertices.Count;
        _vertices.Add(new Vertex(new Vector2(boundsX + i * iconSize, itemHeight), Vector2.Zero, Color.White));
        _vertices.Add(new Vertex(new Vector2(boundsX + i * iconSize + itemIcon.Width, itemHeight), Vector2.Zero, Color.White));
        _vertices.Add(new Vertex(new Vector2(boundsX + i * iconSize + itemIcon.Width, itemHeight + itemIcon.Height), Vector2.Zero, Color.White));
        _vertices.Add(new Vertex(new Vector2(boundsX + i * iconSize, itemHeight + itemIcon.Height), Vector2.Zero, Color.White));
        _indices.AddRange([index, index + 1, index + 2, index, index + 2, index + 3]);
    }

    public void Draw(IndexedImage image, Vector2 position, Rectangle? sourceRectangle, Palette palette, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, int zOrder, float opacity)
    {
        // This is a placeholder. In a real implementation, this would
        // use the palette to color the image and draw it to the screen.
    }
}

public class Vector2
{
    public float X { get; set; }
    public float Y { get; set; }
    public static Vector2 One { get; set; }

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }
    public static Vector2 Zero => new Vector2(0, 0);
    public static implicit operator Vector2(Point point) => new Vector2(point.X, point.Y);
    public static implicit operator Vector2(PointF point) => new Vector2(point.X, point.Y);
    public static implicit operator Vector2(System.Numerics.Vector2 vector) => new Vector2(vector.X, vector.Y);
    public static implicit operator Vector2(System.Drawing.Size size) => new Vector2(size.Width, size.Height);
    public static implicit operator Vector2(System.Drawing.SizeF size) => new Vector2(size.Width, size.Height);
    public static implicit operator Vector2(System.Drawing.Rectangle rectangle) => new Vector2(rectangle.X, rectangle.Y);
    public static implicit operator Vector2(System.Drawing.RectangleF rectangle) => new Vector2(rectangle.X, rectangle.Y);

    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 a, float scalar) => new Vector2(a.X * scalar, a.Y * scalar);
    public static Vector2 operator /(Vector2 a, float scalar)
    {
        if (scalar == 0) throw new DivideByZeroException("Cannot divide by zero.");
        return new Vector2(a.X / scalar, a.Y / scalar);
    }

    public static Vector2 operator *(Vector2 a, Vector2 b) => new Vector2(a.X * b.X, a.Y * b.Y);

    public static float Distance(Vector2 position, Vector2 aiTarget)
    {
        return MathF.Sqrt(MathF.Pow(aiTarget.X - position.X, 2) + MathF.Pow(aiTarget.Y - position.Y, 2));
    }

    public static Vector2 Normalize(Vector2 aiTarget)
    {
        var length = MathF.Sqrt(aiTarget.X * aiTarget.X + aiTarget.Y * aiTarget.Y);
        if (length > 0)
        {
            return new Vector2(aiTarget.X / length, aiTarget.Y / length);
        }
        return new Vector2(0, 0);
    }

    public float Length()
    {
        return MathF.Sqrt(X * X + Y * Y);
    }

    public void Normalize()
    {
        var length = Length();
        if (length > 0)
        {
            X /= length;
            Y /= length;
        }
    }
}
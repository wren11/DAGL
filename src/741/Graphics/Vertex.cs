using System.Drawing;
using System.Numerics;
using Silk.NET.OpenGL;

namespace DarkAges.Library.Graphics;

public struct Vertex(Vector3 position, Vector2 texCoords, Vector4 color)
{
    public Vector3 Position = position;
    public Vector2 TexCoords = texCoords;
    public Vector4 Color = color;

    public const uint SizeInBytes = (3 + 2 + 4) * sizeof(float);

    public Vertex(Vector2 position, Vector2 texCoords, Color color) : this(new Vector3(position.X, position.Y, 0), texCoords, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f))
    {
    }

    public static void Configure(GL gl)
    {
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, SizeInBytes, 0);

        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, SizeInBytes, (3 * sizeof(float)));

        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, SizeInBytes, (5 * sizeof(float)));
    }
}
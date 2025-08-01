using System;
using Silk.NET.OpenGL;

namespace DarkAges.Library.Graphics;

public class VertexArrayObject<TVertexType, TIndexType> : IDisposable 
where TVertexType : unmanaged 
where TIndexType : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;
    private bool _isDisposed;

    public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
    {
        _gl = gl;
        _handle = _gl.GenVertexArray();
        Bind();
        vbo.Bind();
        ebo.Bind();
    }

    public void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
    {
        unsafe
        {
            _gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType), (void*)(offSet * sizeof(TVertexType)));
            _gl.EnableVertexAttribArray(index);
        }
    }

    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _gl.DeleteVertexArray(_handle);
        _isDisposed = true;
    }
}
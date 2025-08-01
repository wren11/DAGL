using System;
using Silk.NET.OpenGL;

namespace DarkAges.Library.Graphics;

public class BufferObject<T> : IDisposable where T : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly BufferTargetARB _bufferType;
    private bool _isDisposed;

    public unsafe BufferObject(GL gl, Vertex[] data, BufferTargetARB bufferType)
    {
        _gl = gl;
        _bufferType = bufferType;
        _handle = _gl.GenBuffer();
        Bind();
        fixed (void* d = data)
        {
            _gl.BufferData(bufferType, (nuint) (data.Length * sizeof(T)), d, BufferUsageARB.StaticDraw);
        }
    }

    public BufferObject(GL gl, uint[] toArray, BufferTargetARB elementArrayBuffer)
    {
        _gl = gl;
        _bufferType = elementArrayBuffer;
        _handle = _gl.GenBuffer();
        Bind();

        unsafe
        {
            fixed (uint* d = toArray)
            {
                _gl.BufferData(elementArrayBuffer, (nuint)(toArray.Length * sizeof(uint)), d, BufferUsageARB.StaticDraw);
            }
        }
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, _handle);
    }

    public void BufferData(Vertex[] data)
    {
        unsafe
        {
            fixed (void* d = data)
            {
                _gl.BufferData(_bufferType, (nuint)(data.Length * sizeof(T)), d, BufferUsageARB.StaticDraw);
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _gl.DeleteBuffer(_handle);
        _isDisposed = true;
    }

    public void BufferData(uint[] toArray)
    {
        unsafe
        {
            fixed (uint* d = toArray)
            {
                _gl.BufferData(_bufferType, (nuint)(toArray.Length * sizeof(uint)), d, BufferUsageARB.StaticDraw);
            }
        }
    }
}
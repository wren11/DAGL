using Silk.NET.OpenGL;
using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Numerics;
using Silk.NET.Maths;

namespace DarkAges.Library.Graphics;

public class Shader : IDisposable
{
    private readonly GL _gl;
    public uint Handle { get; }

    public Shader(GL gl, string vertexPath, string fragmentPath)
    {
        _gl = gl;

        var vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        var fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);

        Handle = _gl.CreateProgram();
        _gl.AttachShader(Handle, vertex);
        _gl.AttachShader(Handle, fragment);
        _gl.LinkProgram(Handle);
        _gl.GetProgram(Handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(Handle)}");
        }

        _gl.DetachShader(Handle, vertex);
        _gl.DetachShader(Handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    public void Use()
    {
        _gl.UseProgram(Handle);
    }

    public void SetUniform(string name, int value)
    {
        var location = _gl.GetUniformLocation(Handle, name);
        if (location != -1)
        {
            _gl.Uniform1(location, value);
        }
    }

    public void SetUniform(string name, Matrix4x4 value)
    {
        var location = _gl.GetUniformLocation(Handle, name);
        if (location != -1)
        {
            unsafe
            {
                _gl.UniformMatrix4(location, 1, false, (float*)&value);
            }
        }
    }

    private uint LoadShader(ShaderType type, string path)
    {
        var src = File.ReadAllText(path);
        var handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);
        var infoLog = _gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Handle);
    }

    public void SetUniform(string projection, double value)
    {

        var location = _gl.GetUniformLocation(Handle, projection);
        if (location != -1)
        {
            _gl.UniformMatrix4(location, 1, false, ref value);
        }
    }

    public void SetUniform(string projection, Matrix4X4<int> value)
    {
        var location = _gl.GetUniformLocation(Handle, projection);
        if (location != -1)
        {
            unsafe
            {
                _gl.UniformMatrix4(location, 1, false, (float*)&value);
            }
        }
    }
}
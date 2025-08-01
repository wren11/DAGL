using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DarkAges.Library.Graphics;

/// <summary>
/// Represents a 16-bit RGB565 color value (5 bits red, 6 bits green, 5 bits blue)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ColorRgb565 : IEquatable<ColorRgb565>
{
    private readonly ushort _value;

    public ColorRgb565(ushort value)
    {
        _value = value;
    }

    public ColorRgb565(byte red, byte green, byte blue)
    {
        // Convert 8-bit values to appropriate bit ranges
        var r = (ushort)((red >> 3) & 0x1F);        // 5 bits
        var g = (ushort)((green >> 2) & 0x3F);      // 6 bits  
        var b = (ushort)((blue >> 3) & 0x1F);       // 5 bits
            
        _value = (ushort)((r << 11) | (g << 5) | b);
    }

    public ColorRgb565(Color color) : this(color.R, color.G, color.B)
    {
    }

    public byte R => (byte)((_value >> 11) << 3);
    public byte G => (byte)(((_value >> 5) & 0x3F) << 2);
    public byte B => (byte)((_value & 0x1F) << 3);

    public ushort Value => _value;

    public Color ToColor()
    {
        return Color.FromArgb(255, R, G, B);
    }

    public static implicit operator Color(ColorRgb565 color)
    {
        return color.ToColor();
    }

    public static implicit operator ColorRgb565(Color color)
    {
        return new ColorRgb565(color);
    }

    public static implicit operator ushort(ColorRgb565 color)
    {
        return color._value;
    }

    public static implicit operator ColorRgb565(ushort value)
    {
        return new ColorRgb565(value);
    }

    public bool Equals(ColorRgb565 other)
    {
        return _value == other._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ColorRgb565 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(ColorRgb565 left, ColorRgb565 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ColorRgb565 left, ColorRgb565 right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"RGB565({R}, {G}, {B})";
    }

    // Common colors
    public static readonly ColorRgb565 Black = new(0, 0, 0);
    public static readonly ColorRgb565 White = new(255, 255, 255);
    public static readonly ColorRgb565 Red = new(255, 0, 0);
    public static readonly ColorRgb565 Green = new(0, 255, 0);
    public static readonly ColorRgb565 Blue = new(0, 0, 255);
    public static readonly ColorRgb565 Yellow = new(255, 255, 0);
    public static readonly ColorRgb565 Magenta = new(255, 0, 255);
    public static readonly ColorRgb565 Cyan = new(0, 255, 255);
    public static readonly ColorRgb565 Gray = new(128, 128, 128);
    public static readonly ColorRgb565 LightGray = new(192, 192, 192);
    public static readonly ColorRgb565 DarkGray = new(64, 64, 64);
    public static readonly ColorRgb565 Transparent = new(0);
}
using System;

namespace DarkAges.Library.Graphics;

/// <summary>
/// Provides static methods for color blending operations.
/// </summary>
public static class ColorBlender
{
    /// <summary>
    /// Performs alpha blending between two RGB 5-6-5 colors.
    /// </summary>
    /// <param name="source">The source color.</param>
    /// <param name="destination">The destination color.</param>
    /// <param name="alpha">The alpha value (0-32), where 32 is fully opaque.</param>
    /// <returns>The blended color.</returns>
    public static ColorRgb565 AlphaBlend(ColorRgb565 source, ColorRgb565 destination, int alpha)
    {
        if (alpha < 0) alpha = 0;
        if (alpha > 32) alpha = 32;

        var invAlpha = 32 - alpha;

        var r = (invAlpha * destination.R + alpha * source.R) >> 5;
        var g = (invAlpha * destination.G + alpha * source.G) >> 5;
        var b = (invAlpha * destination.B + alpha * source.B) >> 5;

        return new ColorRgb565((byte)r, (byte)g, (byte)b);
    }

    /// <summary>
    /// Performs additive blending between two RGB 5-6-5 colors.
    /// </summary>
    /// <param name="source">The source color.</param>
    /// <param name="destination">The destination color.</param>
    /// <returns>The blended color.</returns>
    public static ColorRgb565 AdditiveBlend(ColorRgb565 source, ColorRgb565 destination)
    {
        var r = Math.Min(31, source.R + destination.R);
        var g = Math.Min(63, source.G + destination.G);
        var b = Math.Min(31, source.B + destination.B);

        return new ColorRgb565((byte)r, (byte)g, (byte)b);
    }

    /// <summary>
    /// Blends two 16-bit colors using a 15-bit color as a per-channel alpha mask.
    /// </summary>
    public static ColorRgb565 MaskedBlend(ColorRgb565 source, ColorRgb565 destination, ColorRgb555 mask)
    {
        var r = ((32 - mask.R) * destination.R + mask.R * source.R) / 31;
        var g = ((32 - mask.G) * destination.G + mask.G * source.G) / 31;
        var b = ((32 - mask.B) * destination.B + mask.B * source.B) / 31;

        return new ColorRgb565(
            (byte)Math.Clamp(r, 0, 31),
            (byte)Math.Clamp(g, 0, 63),
            (byte)Math.Clamp(b, 0, 31)
        );
    }

    /// <summary>
    /// Blends two 15-bit colors using another 15-bit color as a per-channel alpha mask.
    /// </summary>
    public static ColorRgb555 MaskedBlend(ColorRgb555 source, ColorRgb555 destination, ColorRgb555 mask)
    {
        var r = ((32 - mask.R) * destination.R + mask.R * source.R) / 31;
        var g = ((32 - mask.G) * destination.G + mask.G * source.G) / 31;
        var b = ((32 - mask.B) * destination.B + mask.B * source.B) / 31;

        return new ColorRgb555(
            (byte)Math.Clamp(r, 0, 31),
            (byte)Math.Clamp(g, 0, 31),
            (byte)Math.Clamp(b, 0, 31)
        );
    }
}
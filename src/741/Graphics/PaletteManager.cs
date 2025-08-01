using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using DarkAges.Library.IO;

namespace DarkAges.Library.Graphics;

/// <summary>
/// Manages color palettes for the game's indexed color system
/// </summary>
public class PaletteManager
{
    private static readonly Dictionary<string, Palette> _palettes = new();
    private static readonly Dictionary<int, Palette> _paletteCache = new();
    private static Palette? _defaultPalette;
    private static bool _isInitialized;

    public static void Initialize()
    {
        if (_isInitialized) return;

        LoadDefaultPalettes();
        _isInitialized = true;
    }

    private static void LoadDefaultPalettes()
    {
        // Create a default 256-color palette
        _defaultPalette = CreateDefaultPalette();
        _palettes["default"] = _defaultPalette;
        _palettes["main"] = _defaultPalette;
            
        // Try to load game palettes
        LoadGamePalettes();
    }

    private static Palette CreateDefaultPalette()
    {
        var colors = new ColorRgb555[256];
            
        // Create a basic color palette
        for (var i = 0; i < 256; i++)
        {
            if (i == 0)
            {
                // Transparent color
                colors[i] = new ColorRgb555(0, 0, 0);
            }
            else if (i < 32)
            {
                // Grayscale
                var gray = (byte)((i - 1) * 8);
                colors[i] = new ColorRgb555(gray, gray, gray);
            }
            else if (i < 64)
            {
                // Red spectrum
                var red = (byte)((i - 32) * 8);
                colors[i] = new ColorRgb555(red, 0, 0);
            }
            else if (i < 96)
            {
                // Green spectrum
                var green = (byte)((i - 64) * 8);
                colors[i] = new ColorRgb555(0, green, 0);
            }
            else if (i < 128)
            {
                // Blue spectrum
                var blue = (byte)((i - 96) * 8);
                colors[i] = new ColorRgb555(0, 0, blue);
            }
            else
            {
                // Mixed colors
                var val = (byte)((i - 128) * 2);
                colors[i] = new ColorRgb555(val, val, val);
            }
        }

        return new Palette(colors);
    }

    private static void LoadGamePalettes()
    {
        try
        {
            // Try to load Dark Ages palette files
            var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (Directory.Exists(dataPath))
            {
                LoadPalettesFromDirectory(dataPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load game palettes: {ex.Message}");
        }
    }

    private static void LoadPalettesFromDirectory(string directory)
    {
        var paletteFiles = Directory.GetFiles(directory, "*.pal", SearchOption.AllDirectories);
            
        foreach (var file in paletteFiles)
        {
            try
            {
                var palette = LoadPaletteFromFile(file);
                if (palette != null)
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    _palettes[name] = palette;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load palette {file}: {ex.Message}");
            }
        }
    }

    private static Palette? LoadPaletteFromFile(string filePath)
    {
        var data = FileManager.Instance.ReadFile(filePath);
        if (data == null || data.Length < 768) // 256 colors * 3 bytes (RGB)
            return null;

        var colors = new ColorRgb555[256];
        for (var i = 0; i < 256; i++)
        {
            var offset = i * 3;
            if (offset + 2 < data.Length)
            {
                var r = data[offset];
                var g = data[offset + 1];
                var b = data[offset + 2];
                colors[i] = new ColorRgb555(r, g, b);
            }
        }

        return new Palette(colors);
    }

    public static Palette GetPalette(string name)
    {
        Initialize();
            
        if (_palettes.TryGetValue(name, out var palette))
            return palette;
                
        // Return default palette if not found
        return _defaultPalette ?? CreateDefaultPalette();
    }

    public static Palette GetPalette(int index)
    {
        Initialize();
            
        if (_paletteCache.TryGetValue(index, out var palette))
            return palette;

        // Try to load by index
        var name = $"palette{index:D3}";
        palette = GetPalette(name);
        _paletteCache[index] = palette;
            
        return palette;
    }

    public static void RegisterPalette(string name, Palette palette)
    {
        if (palette != null)
        {
            _palettes[name] = palette;
        }
    }

    public static void UnloadPalette(string name)
    {
        _palettes.Remove(name);
    }

    public static void Clear()
    {
        _palettes.Clear();
        _paletteCache.Clear();
        _defaultPalette = null;
        _isInitialized = false;
    }

    public static IEnumerable<string> GetAvailablePalettes()
    {
        Initialize();
        return _palettes.Keys;
    }
}
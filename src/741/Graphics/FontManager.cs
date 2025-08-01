using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DarkAges.Library.Graphics;

/// <summary>
/// Manages font loading, caching, and disposal for the game
/// </summary>
public static class FontManager
{
    private static readonly Dictionary<string, SimpleFont> _fonts = new();
    private static readonly Dictionary<string, object> _systemFonts = new();
    private static bool _isInitialized;

    public static void Initialize()
    {
        if (_isInitialized) return;

        // Load default fonts
        LoadDefaultFonts();
        _isInitialized = true;
    }

    private static void LoadDefaultFonts()
    {
        // Create default fonts
        _fonts["default"] = new SimpleFont("default", 12);
        _fonts["small"] = new SimpleFont("small", 10);
        _fonts["large"] = new SimpleFont("large", 16);
        _fonts["title"] = new SimpleFont("title", 24);
            
        // System fonts for compatibility
        _systemFonts["default"] = _fonts["default"];
        _systemFonts["Arial"] = _fonts["default"];
        _systemFonts["Times"] = _fonts["default"];
    }

    public static SimpleFont? GetSimpleFont(string name)
    {
        Initialize();
            
        if (_fonts.TryGetValue(name, out var font))
            return font;
                
        // If not found, return default font
        return _fonts.GetValueOrDefault("default");
    }

    public static SimpleFont? GetFont(string name)
    {
        Initialize();
            
        if (_systemFonts.TryGetValue(name, out var font))
            return (SimpleFont)font;
                
        // Return default font
        return ((SimpleFont)_systemFonts.GetValueOrDefault("default")!);
    }

    public static SimpleFont? LoadFont(string fontPath, int size = 12)
    {
        try
        {
            var fontName = Path.GetFileNameWithoutExtension(fontPath);
            var font = new SimpleFont(fontName, size);
            _fonts[fontName] = font;
            return font;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load font from {fontPath}: {ex.Message}");
            return GetSimpleFont("default");
        }
    }

    public static void RegisterFont(string name, SimpleFont font)
    {
        if (font != null)
        {
            _fonts[name] = font;
            _systemFonts[name] = font;
        }
    }

    public static void UnloadFont(string name)
    {
        if (_fonts.TryGetValue(name, out var font))
        {
            font.Dispose();
            _fonts.Remove(name);
        }
    }

    public static void Dispose()
    {
        foreach (var font in _fonts.Values)
        {
            font.Dispose();
        }
            
        _fonts.Clear();
        _systemFonts.Clear();
        _isInitialized = false;
    }

    public static IEnumerable<string> GetAvailableFonts()
    {
        Initialize();
        return _fonts.Keys;
    }

    public class Instance
    {
        private static readonly Lazy<Instance> _instance = new(() => new Instance());
        public static Instance Current => _instance.Value;
        private Instance()
        {
            FontManager.Initialize();
        }
        public static SimpleFont? GetFont(string name) => FontManager.GetSimpleFont(name);

        public void Dispose() => FontManager.Dispose();
    }
}
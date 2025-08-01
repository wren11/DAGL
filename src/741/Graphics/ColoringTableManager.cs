using System;
using System.Collections.Generic;
using System.IO;
using DarkAges.Library.IO;

namespace DarkAges.Library.Graphics;

/// <summary>
/// Manages color transformation tables for character customization and effects
/// </summary>
public static class ColoringTableManager
{
    private static readonly Dictionary<string, ColoringTable> _tables = new();
    private static readonly Dictionary<int, ColoringTable> _tableCache = new();
    private static bool _isInitialized;

    public static void Initialize()
    {
        if (_isInitialized) return;

        LoadDefaultTables();
        LoadGameTables();
        _isInitialized = true;
    }

    private static void LoadDefaultTables()
    {
        // Create default identity tables
        var identityTable = CreateIdentityTable();
        _tables["identity"] = identityTable;
        _tables["default"] = identityTable;

        // Create hair color tables
        CreateHairColorTables();
            
        // Create skin color tables  
        CreateSkinColorTables();
            
        // Create clothing color tables
        CreateClothingColorTables();
    }

    private static ColoringTable CreateIdentityTable()
    {
        var mapping = new byte[256];
        for (var i = 0; i < 256; i++)
        {
            mapping[i] = (byte)i;
        }
        return new ColoringTable(mapping);
    }

    private static void CreateHairColorTables()
    {
        // Create different hair color transformations
        var hairColors = new[]
        {
            ("hair_1", [0, 0, 0]),        // Black
            ("hair_2", [139, 69, 19]),   // Brown
            ("hair_3", [255, 215, 0]),   // Blonde
            ("hair_4", [255, 0, 0]),     // Red
            ("hair_5", [128, 128, 128]), // Gray
            ("hair_6", new byte[] { 255, 255, 255 }), // White
        };

        foreach (var (name, color) in hairColors)
        {
            _tables[name] = CreateColorShiftTable(color[0], color[1], color[2]);
        }
    }

    private static void CreateSkinColorTables()
    {
        // Create different skin tone transformations
        var skinTones = new[]
        {
            ("skin_1", [255, 220, 177]), // Light
            ("skin_2", [241, 194, 125]), // Medium
            ("skin_3", [198, 134, 66]),  // Tan
            ("skin_4", [141, 85, 36]),   // Dark
            ("skin_5", new byte[] { 92, 51, 23 }),    // Very Dark
        };

        foreach (var (name, color) in skinTones)
        {
            _tables[name] = CreateColorShiftTable(color[0], color[1], color[2]);
        }
    }

    private static void CreateClothingColorTables()
    {
        // Create clothing color transformations
        var clothingColors = new[]
        {
            ("cloth_red", [255, 0, 0]),
            ("cloth_blue", [0, 0, 255]),
            ("cloth_green", [0, 255, 0]),
            ("cloth_yellow", [255, 255, 0]),
            ("cloth_purple", [128, 0, 128]),
            ("cloth_orange", [255, 165, 0]),
            ("cloth_pink", [255, 192, 203]),
            ("cloth_cyan", new byte[] { 0, 255, 255 }),
        };

        foreach (var (name, color) in clothingColors)
        {
            _tables[name] = CreateColorShiftTable(color[0], color[1], color[2]);
        }
    }

    private static ColoringTable CreateColorShiftTable(byte targetR, byte targetG, byte targetB)
    {
        var mapping = new byte[256];
            
        for (var i = 0; i < 256; i++)
        {
            if (i == 0)
            {
                mapping[i] = 0; // Keep transparent
            }
            else
            {
                // Simple color shift based on intensity
                var intensity = i / 255.0f;
                var newR = (byte)(targetR * intensity);
                var newG = (byte)(targetG * intensity);
                var newB = (byte)(targetB * intensity);
                    
                // Find closest palette index (simplified)
                mapping[i] = (byte)Math.Min(255, (newR + newG + newB) / 3);
            }
        }
            
        return new ColoringTable(mapping);
    }

    private static void LoadGameTables()
    {
        try
        {
            var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (Directory.Exists(dataPath))
            {
                LoadTablesFromDirectory(dataPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load game coloring tables: {ex.Message}");
        }
    }

    private static void LoadTablesFromDirectory(string directory)
    {
        // Look for coloring table files (.col, .tbl, etc.)
        var extensions = new[] { "*.col", "*.tbl", "*.clt" };
            
        foreach (var extension in extensions)
        {
            var files = Directory.GetFiles(directory, extension, SearchOption.AllDirectories);
                
            foreach (var file in files)
            {
                try
                {
                    var table = LoadTableFromFile(file);
                    if (table != null)
                    {
                        var name = Path.GetFileNameWithoutExtension(file);
                        _tables[name] = table;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load coloring table {file}: {ex.Message}");
                }
            }
        }
    }

    private static ColoringTable? LoadTableFromFile(string filePath)
    {
        var data = FileManager.Instance.ReadFile(filePath);
        if (data == null || data.Length < 256)
            return null;

        var mapping = new byte[256];
        Array.Copy(data, 0, mapping, 0, Math.Min(256, data.Length));
            
        return new ColoringTable(mapping);
    }

    public static ColoringTable? GetTable(string name)
    {
        Initialize();
            
        if (_tables.TryGetValue(name, out var table))
            return table;
                
        // Return null if not found (unlike PaletteManager)
        return null;
    }

    public static ColoringTable GetTable(int index)
    {
        Initialize();
            
        if (_tableCache.TryGetValue(index, out var table))
            return table;

        // Try to load by index
        var name = $"table{index:D3}";
        table = GetTable(name) ?? CreateIdentityTable();
        _tableCache[index] = table;
            
        return table;
    }

    public static void RegisterTable(string name, ColoringTable table)
    {
        if (table != null)
        {
            _tables[name] = table;
        }
    }

    public static void UnloadTable(string name)
    {
        _tables.Remove(name);
    }

    public static void Clear()
    {
        _tables.Clear();
        _tableCache.Clear();
        _isInitialized = false;
    }

    public static void Dispose()
    {
        Clear();
    }

    public static IEnumerable<string> GetAvailableTables()
    {
        Initialize();
        return _tables.Keys;
    }
}
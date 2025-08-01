using System;
using System.Drawing;

namespace DarkAges.Library.Graphics;

public class Palette
{
    public ColorRgb[] Colors { get; private set; }
    public int ColorCount { get; private set; }

    public Palette()
    {
        Colors = new ColorRgb[256];
        ColorCount = 0;
    }

    public Palette(int colorCount)
    {
        if (colorCount < 0 || colorCount > 256)
            throw new ArgumentOutOfRangeException(nameof(colorCount), "Color count must be between 0 and 256");

        Colors = new ColorRgb[256];
        ColorCount = colorCount;
    }

    public Palette(ColorRgb555[] colorCount)
    {
        if (colorCount == null || colorCount.Length < 0 || colorCount.Length > 256)
            throw new ArgumentOutOfRangeException(nameof(colorCount), "Color count must be between 0 and 256");
        Colors = new ColorRgb[256];
        ColorCount = colorCount.Length;
        for (var i = 0; i < ColorCount; i++)
        {
            Colors[i] = new ColorRgb(colorCount[i].R, colorCount[i].G, colorCount[i].B);
        }
    }

    public Palette(Palette? other)
    {
        Colors = new ColorRgb[256];
        if (other == null) return;
        ColorCount = other.ColorCount;
        for (var i = 0; i < ColorCount; i++)
        {
            Colors[i] = other.Colors[i];
        }
    }

    public void ApplyColoringTable(ColoringTable table)
    {
        if (table == null) return;

        for (var i = 0; i < table.Tuples.Count; i++)
        {
            var tuple = table.Tuples[i];
            if (tuple.Index >= 0 && tuple.Index < ColorCount)
            {
                Colors[tuple.Index] = tuple.Color;
            }
        }
    }

    public Color GetColor(int index)
    {
        if (index < 0 || index >= ColorCount)
            return Color.Black;

        return Colors[index];
    }

    public void Dispose()
    {
        // This is a placeholder. In a real implementation, this would
        // release any resources used by the palette.
    }

    public void SetColor(int index, Color color)
    {
        if (index < 0 || index >= 256)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 255");

        Colors[index] = color;
        if (index >= ColorCount)
            ColorCount = index + 1;
    }

    public void LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Palette file not found: {filePath}");

            var extension = Path.GetExtension(filePath).ToLower();
                
            switch (extension)
            {
            case ".pal":
                LoadPalFile(filePath);
                break;
            case ".act":
                LoadActFile(filePath);
                break;
            case ".lbm":
                LoadLbmFile(filePath);
                break;
            default:
                throw new NotSupportedException($"Unsupported palette format: {extension}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load palette from {filePath}: {ex.Message}");
            LoadDefaultPalette();
        }
    }

    public void SaveToFile(string filePath)
    {
        try
        {
            var extension = Path.GetExtension(filePath).ToLower();
                
            switch (extension)
            {
            case ".pal":
                SavePalFile(filePath);
                break;
            case ".act":
                SaveActFile(filePath);
                break;
            default:
                throw new NotSupportedException($"Unsupported palette format for saving: {extension}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save palette to {filePath}: {ex.Message}");
        }
    }

    private void LoadPalFile(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));
        // Read RIFF header
        var riff = reader.ReadChars(4);
        if (new string(riff) != "RIFF")
            throw new InvalidDataException("Invalid PAL file format");

        reader.ReadInt32(); // file size
        var pal = reader.ReadChars(4);
        if (new string(pal) != "PAL ")
            throw new InvalidDataException("Invalid PAL file format");

        var data = reader.ReadChars(4);
        if (new string(data) != "data")
            throw new InvalidDataException("Invalid PAL file format");

        var dataSize = reader.ReadInt32();
        var version = reader.ReadInt16();
        var colorCount = reader.ReadInt16();

        Colors = new ColorRgb[colorCount];
        ColorCount = colorCount;

        for (var i = 0; i < colorCount; i++)
        {
            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            var flags = reader.ReadByte();
                    
            Colors[i] = new ColorRgb(r, g, b);
        }
    }

    private void LoadActFile(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));
        Colors = new ColorRgb[256];
        ColorCount = 256;

        for (var i = 0; i < 256; i++)
        {
            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
                    
            Colors[i] = new ColorRgb(r, g, b);
        }
    }

    private void LoadLbmFile(string filePath)
    {
        // Load LBM/IFF palette format (simplified)
        using var reader = new BinaryReader(File.OpenRead(filePath));
        // Skip to CMAP chunk
        while (reader.BaseStream.Position < reader.BaseStream.Length - 8)
        {
            var chunk = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();
                    
            if (chunk == "CMAP")
            {
                var colorCount = chunkSize / 3;
                Colors = new ColorRgb[colorCount];
                ColorCount = colorCount;
                        
                for (var i = 0; i < colorCount; i++)
                {
                    var r = reader.ReadByte();
                    var g = reader.ReadByte();
                    var b = reader.ReadByte();
                    Colors[i] = new ColorRgb(r, g, b);
                }
                return;
            }
            else
            {
                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
            }
        }
    }

    private void SavePalFile(string filePath)
    {
        using var writer = new BinaryWriter(File.Create(filePath));
        // Write RIFF header
        writer.Write("RIFF".ToCharArray());
        writer.Write(24 + ColorCount * 4); // file size
        writer.Write("PAL ".ToCharArray());
        writer.Write("data".ToCharArray());
        writer.Write(4 + ColorCount * 4); // data size
        writer.Write((short)0x0300); // version
        writer.Write((short)ColorCount);

        for (var i = 0; i < ColorCount; i++)
        {
            writer.Write(Colors[i].R);
            writer.Write(Colors[i].G);
            writer.Write(Colors[i].B);
            writer.Write((byte)0); // flags
        }
    }

    private void SaveActFile(string filePath)
    {
        using var writer = new BinaryWriter(File.Create(filePath));
        for (var i = 0; i < Math.Min(256, ColorCount); i++)
        {
            writer.Write(Colors[i].R);
            writer.Write(Colors[i].G);
            writer.Write(Colors[i].B);
        }
                
        // Pad to 256 colors if necessary
        for (var i = ColorCount; i < 256; i++)
        {
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
    }

    private void LoadDefaultPalette()
    {
        // Create a default VGA palette
        Colors = new ColorRgb[256];
        ColorCount = 256;
            
        // Standard VGA palette
        for (var i = 0; i < 16; i++)
        {
            Colors[i] = GetVgaColor(i);
        }
            
        // Grayscale ramp
        for (var i = 16; i < 256; i++)
        {
            var gray = (byte)((i - 16) * 255 / 239);
            Colors[i] = new ColorRgb(gray, gray, gray);
        }
    }

    private ColorRgb GetVgaColor(int index)
    {
        return index switch
        {
            0 => new ColorRgb(0, 0, 0),       // Black
            1 => new ColorRgb(0, 0, 128),     // Dark Blue
            2 => new ColorRgb(0, 128, 0),     // Dark Green
            3 => new ColorRgb(0, 128, 128),   // Dark Cyan
            4 => new ColorRgb(128, 0, 0),     // Dark Red
            5 => new ColorRgb(128, 0, 128),   // Dark Magenta
            6 => new ColorRgb(128, 128, 0),   // Brown
            7 => new ColorRgb(192, 192, 192), // Light Gray
            8 => new ColorRgb(128, 128, 128), // Dark Gray
            9 => new ColorRgb(0, 0, 255),     // Blue
            10 => new ColorRgb(0, 255, 0),    // Green
            11 => new ColorRgb(0, 255, 255),  // Cyan
            12 => new ColorRgb(255, 0, 0),    // Red
            13 => new ColorRgb(255, 0, 255),  // Magenta
            14 => new ColorRgb(255, 255, 0),  // Yellow
            15 => new ColorRgb(255, 255, 255), // White
            _ => new ColorRgb(0, 0, 0)
        };
    }
}
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace DarkAges.Library.IO;

public class BmpReader
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[] PixelData { get; private set; }
    public List<Color> Palette { get; private set; }
    public int BitsPerPixel { get; private set; }

    public BmpReader(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);
        // BMP Header
        if (reader.ReadByte() != 'B' || reader.ReadByte() != 'M')
            throw new InvalidDataException("Not a BMP file");

        reader.ReadInt32(); // file size
        reader.ReadInt16(); // reserved1
        reader.ReadInt16(); // reserved2
        var pixelDataOffset = reader.ReadInt32();

        // DIB Header
        var headerSize = reader.ReadInt32();
        Width = reader.ReadInt32();
        Height = reader.ReadInt32();
        reader.ReadInt16(); // color planes
        BitsPerPixel = reader.ReadInt16();
        var compression = reader.ReadInt32();
        if (compression != 0)
            throw new NotSupportedException("Compressed BMPs are not supported");
        var imageSize = reader.ReadInt32();
        reader.ReadInt32(); // x pixels per meter
        reader.ReadInt32(); // y pixels per meter
        var colorsUsed = reader.ReadInt32();
        reader.ReadInt32(); // important colors

        Palette = [];
        if (BitsPerPixel == 8)
        {
            var paletteSize = colorsUsed == 0 ? 256 : colorsUsed;
            for (var i = 0; i < paletteSize; i++)
            {
                var b = reader.ReadByte();
                var g = reader.ReadByte();
                var r = reader.ReadByte();
                reader.ReadByte(); // reserved
                Palette.Add(Color.FromArgb(r, g, b));
            }
        }

        stream.Seek(pixelDataOffset, SeekOrigin.Begin);
        var rowSize = ((BitsPerPixel * Width + 31) / 32) * 4;
        PixelData = new byte[Width * Height * (BitsPerPixel / 8)];

        if (BitsPerPixel == 8)
        {
            for (var y = Height - 1; y >= 0; y--)
            {
                var rowStart = y * Width;
                var row = reader.ReadBytes(rowSize);
                Array.Copy(row, 0, PixelData, rowStart, Width);
            }
        }
        else if (BitsPerPixel == 24)
        {
            for (var y = Height - 1; y >= 0; y--)
            {
                var rowStart = y * Width * 3;
                var row = reader.ReadBytes(rowSize);
                Array.Copy(row, 0, PixelData, rowStart, Width * 3);
            }
        }
        else
        {
            throw new NotSupportedException($"Unsupported BMP bit depth: {BitsPerPixel}");
        }
    }
}
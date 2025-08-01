using System;
using System.Drawing;
using System.IO;

namespace DarkAges.Library.Graphics;

public static class PcxReader
{
    public static Image? LoadImage(string fileName)
    {
        try
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"PCX file not found: {fileName}");
                return null;
            }

            using var stream = File.OpenRead(fileName);
            using var reader = new BinaryReader(stream);
                
            // Read PCX header
            var manufacturer = reader.ReadByte();
            var version = reader.ReadByte();
            var encoding = reader.ReadByte();
            var bitsPerPixel = reader.ReadByte();
                
            var xMin = reader.ReadInt16();
            var yMin = reader.ReadInt16();
            var xMax = reader.ReadInt16();
            var yMax = reader.ReadInt16();
                
            var hDpi = reader.ReadInt16();
            var vDpi = reader.ReadInt16();
                
            // Skip color palette info
            stream.Seek(48, SeekOrigin.Current);
                
            var reserved = reader.ReadByte();
            var colorPlanes = reader.ReadByte();
            var bytesPerLine = reader.ReadInt16();
            var paletteType = reader.ReadInt16();
                
            // Skip padding
            stream.Seek(58, SeekOrigin.Current);
                
            var width = xMax - xMin + 1;
            var height = yMax - yMin + 1;
                
            // For simplicity, create a basic bitmap
            var bitmap = new Bitmap(width, height);
                
            // Read image data (simplified - would need proper RLE decoding for real PCX files)
            var imageData = new byte[width * height];
            var dataLength = (int)(stream.Length - stream.Position);
            var compressedData = reader.ReadBytes(dataLength);
                
            // Simple RLE decompression (basic implementation)
            var dataIndex = 0;
            var pixelIndex = 0;
                
            while (dataIndex < compressedData.Length && pixelIndex < imageData.Length)
            {
                var byte1 = compressedData[dataIndex++];
                    
                if ((byte1 & 0xC0) == 0xC0)
                {
                    // RLE encoded
                    var count = byte1 & 0x3F;
                    var value = compressedData[dataIndex++];
                        
                    for (var i = 0; i < count && pixelIndex < imageData.Length; i++)
                    {
                        imageData[pixelIndex++] = value;
                    }
                }
                else
                {
                    // Raw byte
                    imageData[pixelIndex++] = byte1;
                }
            }
                
            // Convert to bitmap (simplified - assumes 8-bit indexed)
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pixelValue = imageData[y * width + x];
                    var color = Color.FromArgb(pixelValue, pixelValue, pixelValue);
                    bitmap.SetPixel(x, y, color);
                }
            }
                
            return bitmap;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load PCX file {fileName}: {ex.Message}");
            return null;
        }
    }
}
using DarkAges.Library.Graphics;
using System.IO;
using System.Runtime.InteropServices;

namespace DarkAges.Library.IO;

public static class PcxReader
{
    public static IndexedImage Load(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);
        var headerBytes = reader.ReadBytes(Marshal.SizeOf<PcxHeader>());
        var handle = GCHandle.Alloc(headerBytes, GCHandleType.Pinned);
        var header = Marshal.PtrToStructure<PcxHeader>(handle.AddrOfPinnedObject());
        handle.Free();

        var width = header.XMax - header.XMin + 1;
        var height = header.YMax - header.YMin + 1;
        var pixelData = new byte[width * height];

        DecodeRle(reader, pixelData, width, height, header.NPlanes, header.BytesPerLine);
                
        return new IndexedImage(width, height, pixelData);
    }

    private static void DecodeRle(BinaryReader reader, byte[] pixelData, int width, int height, int planes, int bytesPerLine)
    {
        var dataIndex = 0;
        for (var p = 0; p < planes; p++)
        {
            for (var y = 0; y < height; y++)
            {
                var x = 0;
                while (x < bytesPerLine)
                {
                    var b = reader.ReadByte();
                    int runLength;
                    byte runValue;

                    if ((b & 0xC0) == 0xC0)
                    {
                        runLength = b & 0x3F;
                        runValue = reader.ReadByte();
                    }
                    else
                    {
                        runLength = 1;
                        runValue = b;
                    }

                    for (var i = 0; i < runLength; i++)
                    {
                        if (x < width)
                        {
                            var pixelIndex = (y * width) + x;
                            if(pixelIndex < pixelData.Length)
                                pixelData[pixelIndex] = runValue;
                        }
                        x++;
                    }
                }
            }
        }
    }
}
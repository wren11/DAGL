using System;
using System.Collections.Generic;
using System.IO;

namespace DarkAges.Library.IO;

public class EfaFile
{
    public int FrameCount { get; private set; }
    public float FrameDuration { get; private set; } // seconds per frame
    public List<EfaFrame> Frames { get; } = [];
    public byte[] RawData { get; private set; }

    public EfaFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"EFA file not found: {filePath}");

        RawData = File.ReadAllBytes(filePath);
        Parse(RawData);
    }

    private void Parse(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        // Header parsing (based on typical effect animation format)
        FrameCount = reader.ReadInt16();
        FrameDuration = reader.ReadSingle(); // seconds per frame
        // Skip unknowns/padding
        stream.Seek(0x20, SeekOrigin.Begin);

        // Parse frames
        for (var i = 0; i < FrameCount; i++)
        {
            int width = reader.ReadInt16();
            int height = reader.ReadInt16();
            var dataSize = reader.ReadInt32();
            var dataOffset = stream.Position;
            var frameData = reader.ReadBytes(dataSize);
            Frames.Add(new EfaFrame
            {
                Width = width,
                Height = height,
                Data = frameData
            });
        }
    }

    public byte[] GetFramePixels(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= FrameCount)
            return null;
            
        var frame = Frames[frameIndex];
            
        // Check if data needs decompression
        if (IsCompressed(frame.Data))
        {
            return DecompressFrameData(frame);
        }
            
        // Return raw data if not compressed
        return frame.Data;
    }

    private bool IsCompressed(byte[] data)
    {
        if (data.Length < 4) return false;
            
        // Check for common compression signatures
        // RLE compression often starts with specific markers
        var firstByte = data[0];
        var secondByte = data[1];
            
        // Common RLE markers or compression flags
        return (firstByte == 0xFF && secondByte == 0xFE) || // RLE marker
                (firstByte == 0x78 && (secondByte == 0x9C || secondByte == 0xDA)) || // zlib/deflate
                (data.Length >= 2 && data[0] == 0x1F && data[1] == 0x8B); // gzip
    }

    private byte[] DecompressFrameData(EfaFrame frame)
    {
        var data = frame.Data;
            
        // Try different decompression methods
        try
        {
            // First try RLE decompression
            if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE)
            {
                return DecompressRLE(data, frame.Width * frame.Height);
            }
                
            // Try zlib/deflate decompression
            if (data.Length >= 2 && data[0] == 0x78 && (data[1] == 0x9C || data[1] == 0xDA))
            {
                return DecompressZlib(data);
            }
                
            // Try gzip decompression
            if (data.Length >= 2 && data[0] == 0x1F && data[1] == 0x8B)
            {
                return DecompressGzip(data);
            }
                
            // If no specific compression detected, try generic RLE
            return DecompressGenericRLE(data, frame.Width * frame.Height);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to decompress frame data: {ex.Message}");
            return data; // Return raw data as fallback
        }
    }

    private byte[] DecompressRLE(byte[] data, int expectedSize)
    {
        var result = new List<byte>();
        var index = 2; // Skip RLE marker
            
        while (index < data.Length && result.Count < expectedSize)
        {
            if (index + 1 >= data.Length) break;
                
            var count = data[index++];
            var value = data[index++];
                
            // Handle run-length encoding
            if (count == 0)
            {
                // Single byte
                result.Add(value);
            }
            else
            {
                // Repeated bytes
                for (var i = 0; i < count; i++)
                {
                    result.Add(value);
                    if (result.Count >= expectedSize) break;
                }
            }
        }
            
        return result.ToArray();
    }

    private byte[] DecompressGenericRLE(byte[] data, int expectedSize)
    {
        var result = new List<byte>();
        var index = 0;
            
        while (index < data.Length && result.Count < expectedSize)
        {
            if (index + 1 >= data.Length) break;
                
            var header = data[index++];
                
            if ((header & 0x80) != 0)
            {
                // Compressed run
                var count = (header & 0x7F) + 1;
                if (index >= data.Length) break;
                var value = data[index++];
                    
                for (var i = 0; i < count && result.Count < expectedSize; i++)
                {
                    result.Add(value);
                }
            }
            else
            {
                // Literal run
                var count = header + 1;
                for (var i = 0; i < count && index < data.Length && result.Count < expectedSize; i++)
                {
                    result.Add(data[index++]);
                }
            }
        }
            
        return result.ToArray();
    }

    private byte[] DecompressZlib(byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var deflateStream = new System.IO.Compression.DeflateStream(compressedStream, System.IO.Compression.CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        // Skip zlib header (2 bytes)
        compressedStream.Seek(2, SeekOrigin.Begin);
        deflateStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }

    private byte[] DecompressGzip(byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var gzipStream = new System.IO.Compression.GZipStream(compressedStream, System.IO.Compression.CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        gzipStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}
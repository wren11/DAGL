using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.IO;

public class SpfFile
{
    public short FrameCount { get; private set; }
    public List<SpfFrameHeader> Frames { get; } = [];
    private byte[] _fileData;
    private int _dataOffset;

    public SpfFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"SPF file not found: {filePath}");

        _fileData = File.ReadAllBytes(filePath);
        ParseHeader();
    }

    private void ParseHeader()
    {
        using var stream = new MemoryStream(_fileData);
        using var reader = new BinaryReader(stream);
        // Read frame count (first 2 bytes)
        FrameCount = reader.ReadInt16();
                
        // Skip the next 6 bytes (header padding)
        reader.ReadBytes(6);
                
        // Check if this is a compressed format
        var isCompressed = reader.ReadInt16() == 0;
                
        if (isCompressed)
        {
            // Skip to the frame table
            reader.BaseStream.Position = 1036;
        }
        else
        {
            // Frame table starts at offset 12
            reader.BaseStream.Position = 12;
        }
                
        // Read frame count from table
        var tableFrameCount = reader.ReadInt32();
        FrameCount = (short)Math.Min(FrameCount, tableFrameCount);
                
        // Skip frame table header (32 bytes)
        reader.ReadBytes(32);
                
        // Read frame headers
        for (var i = 0; i < FrameCount; i++)
        {
            var frame = new SpfFrameHeader
            {
                X1 = reader.ReadInt16(),
                Y1 = reader.ReadInt16(),
                X2 = reader.ReadInt16(),
                Y2 = reader.ReadInt16(),
                DataOffset = reader.ReadInt32(),
                CompressedSize = reader.ReadInt32()
            };
                    
            Frames.Add(frame);
        }
                
        _dataOffset = (int)reader.BaseStream.Position;
    }

    public IndexedImage GetFrame(short frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= FrameCount)
            return null;

        var frame = Frames[frameIndex];
            
        // Calculate frame dimensions
        var width = frame.X2 - frame.X1;
        var height = frame.Y2 - frame.Y1;
            
        if (width <= 0 || height <= 0)
            return null;

        // Extract compressed data
        var compressedData = new byte[frame.CompressedSize];
        Array.Copy(_fileData, frame.DataOffset, compressedData, 0, frame.CompressedSize);
            
        // Decompress the data
        var pixelData = DecompressFrameData(compressedData, width * height);
            
        return new IndexedImage(width, height, pixelData);
    }

    private byte[] DecompressFrameData(byte[] compressedData, int expectedSize)
    {
        try
        {
            // Try standard compression first
            using var compressedStream = new MemoryStream(compressedData);
            using var decompressStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            decompressStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
        catch
        {
            // If standard decompression fails, try custom RLE decompression
            return DecompressRLE(compressedData, expectedSize);
        }
    }

    private byte[] DecompressRLE(byte[] compressedData, int expectedSize)
    {
        var result = new byte[expectedSize];
        var resultIndex = 0;
        var dataIndex = 0;
            
        while (resultIndex < expectedSize && dataIndex < compressedData.Length)
        {
            var count = compressedData[dataIndex++];
                
            if (count == 0)
            {
                // Literal run
                if (dataIndex < compressedData.Length)
                {
                    result[resultIndex++] = compressedData[dataIndex++];
                }
            }
            else if (count < 128)
            {
                // Copy run
                for (var i = 0; i < count && resultIndex < expectedSize; i++)
                {
                    if (dataIndex < compressedData.Length)
                    {
                        result[resultIndex++] = compressedData[dataIndex++];
                    }
                }
            }
            else
            {
                // RLE run
                count = (byte)(256 - count);
                if (dataIndex < compressedData.Length)
                {
                    var value = compressedData[dataIndex++];
                    for (var i = 0; i < count && resultIndex < expectedSize; i++)
                    {
                        result[resultIndex++] = value;
                    }
                }
            }
        }
            
        return result;
    }

    public void Dispose()
    {
        _fileData = null;
        Frames.Clear();
    }
}
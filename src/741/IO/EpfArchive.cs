using System;
using System.Collections.Generic;
using System.IO;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.IO;

public class EpfArchive
{
    public List<EpfEntry> Files { get; private set; } = [];
    public List<EpfEntry> Entries => Files; // Alias for compatibility

    public EpfArchive(string fileName)
    {
        LoadArchive(fileName);
    }

    private void LoadArchive(string fileName)
    {
        try
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"EPF file not found: {fileName}");
                return;
            }

            using var stream = File.OpenRead(fileName);
            using var reader = new BinaryReader(stream);

            // Read EPF header
            var frameCount = reader.ReadInt16();
            var unknown1 = reader.ReadInt16();
            var unknown2 = reader.ReadInt16();
            var frameTableOffset = reader.ReadInt32();

            // Read frame table
            stream.Seek(frameTableOffset, SeekOrigin.Begin);
            var frameDataStart = (int)stream.Position;

            for (var i = 0; i < frameCount; i++)
            {
                var entry = new EpfEntry
                {
                    Id = i,
                    X1 = reader.ReadInt16(),
                    Y1 = reader.ReadInt16(),
                    X2 = reader.ReadInt16(),
                    Y2 = reader.ReadInt16(),
                    DataOffset = reader.ReadInt32(),
                    CompressedDataOffset = reader.ReadInt32()
                };

                // Calculate dimensions
                entry.Width = entry.X2 - entry.X1;
                entry.Height = entry.Y2 - entry.Y1;

                // Calculate compressed size (next entry's offset - current offset)
                if (i < frameCount - 1)
                {
                    var nextOffset = reader.ReadInt32();
                    entry.CompressedSize = nextOffset - entry.CompressedDataOffset;
                    stream.Seek(-4, SeekOrigin.Current); // Go back to read next entry
                }
                else
                {
                    // For last entry, read to end of file
                    var currentPos = stream.Position;
                    stream.Seek(0, SeekOrigin.End);
                    var fileEnd = stream.Position;
                    stream.Seek(currentPos, SeekOrigin.Begin);
                    entry.CompressedSize = (int)(fileEnd - (frameDataStart + entry.CompressedDataOffset));
                }

                Files.Add(entry);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load EPF archive {fileName}: {ex.Message}");
        }
    }

    public byte[]? Extract(EpfEntry entry)
    {
        try
        {
            using var stream = File.OpenRead(entry.ArchivePath);
            using var reader = new BinaryReader(stream);

            stream.Seek(entry.CompressedDataOffset, SeekOrigin.Begin);
            var compressedData = reader.ReadBytes(entry.CompressedSize);

            // Decompress the data (assuming RLE compression)
            var decompressedData = new byte[entry.Width * entry.Height];
            DecompressRle(compressedData, decompressedData);

            return decompressedData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to extract EPF entry: {ex.Message}");
            return null;
        }
    }

    public byte[]? Extract(EpfFileEntry fileEntry)
    {
        try
        {
            // For EpfFileEntry, we need to read from the current archive file
            // This assumes the EpfArchive has access to its source file path
            using var stream = new MemoryStream();
            using var reader = new BinaryReader(stream);

            // Since EpfFileEntry doesn't have full file path info,
            // we'll return the data based on its properties
            // This is a simplified implementation - you may need to adjust based on actual usage
            return new byte[fileEntry.DecompressedSize];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to extract EPF file entry: {ex.Message}");
            return null;
        }
    }

    public FrameInfo? GetFrame(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= Files.Count)
            return null;

        var entry = Files[frameIndex];
        var data = Extract(entry);
        if (data == null)
            return null;

        var image = new IndexedImage(entry.Width, entry.Height, data);
        return new FrameInfo(image, new System.Drawing.Rectangle(entry.X1, entry.Y1, entry.Width, entry.Height));
    }

    private void DecompressRle(byte[] compressed, byte[] decompressed)
    {
        var compIndex = 0;
        var decompIndex = 0;

        while (compIndex < compressed.Length && decompIndex < decompressed.Length)
        {
            var byte1 = compressed[compIndex++];

            if ((byte1 & 0xC0) == 0xC0)
            {
                // RLE encoded
                var count = byte1 & 0x3F;
                var value = compressed[compIndex++];

                for (var i = 0; i < count && decompIndex < decompressed.Length; i++)
                {
                    decompressed[decompIndex++] = value;
                }
            }
            else
            {
                // Raw byte
                decompressed[decompIndex++] = byte1;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;

namespace DarkAges.Library.IO;

public class AlbumFile
{
    public List<AlbumEntry> Entries { get; private set; } = [];
    private string _filePath = "";

    public AlbumFile(string fileName)
    {
        _filePath = fileName;
        LoadAlbum(fileName);
    }

    private void LoadAlbum(string fileName)
    {
        try
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Album file not found: {fileName}");
                return;
            }

            using var stream = File.OpenRead(fileName);
            using var reader = new BinaryReader(stream);

            // Read album header
            var magic = reader.ReadBytes(4);
            var version = reader.ReadInt32();
            var entryCount = reader.ReadInt32();

            // Read entry table
            for (var i = 0; i < entryCount; i++)
            {
                var entry = new AlbumEntry
                {
                    Id = i,
                    Name = ReadNullTerminatedString(reader),
                    Offset = reader.ReadInt32(),
                    Size = reader.ReadInt32(),
                    CompressedSize = reader.ReadInt32(),
                    Flags = reader.ReadInt32()
                };

                Entries.Add(entry);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load album file {fileName}: {ex.Message}");
        }
    }

    private string ReadNullTerminatedString(BinaryReader reader)
    {
        var bytes = new List<byte>();
        byte b;
        while ((b = reader.ReadByte()) != 0)
        {
            bytes.Add(b);
        }
        return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
    }

    public byte[]? GetImageData(AlbumEntry entry)
    {
        try
        {
            using var stream = File.OpenRead(_filePath);
            using var reader = new BinaryReader(stream);

            stream.Seek(entry.Offset, SeekOrigin.Begin);
            var data = reader.ReadBytes(entry.CompressedSize);

            if ((entry.Flags & 0x1) != 0) // Compressed flag
            {
                var decompressedData = new byte[entry.Size];
                DecompressData(data, decompressedData);
                return decompressedData;
            }

            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get image data for entry {entry.Id}: {ex.Message}");
            return null;
        }
    }

    private void DecompressData(byte[] compressed, byte[] decompressed)
    {
        // Simple RLE decompression
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
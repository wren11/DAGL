using System;
using System.Collections.Generic;
using System.IO;

namespace DarkAges.Library.IO;

public class SotpFile
{
    public List<SotpObjectTemplate> Templates { get; } = [];
    public byte[] RawData { get; private set; }

    public SotpFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"SOTP file not found: {filePath}");

        RawData = File.ReadAllBytes(filePath);
        Parse(RawData);
    }

    private void Parse(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        // Header parsing (based on typical static object template format)
        int count = reader.ReadInt16();
        for (var i = 0; i < count; i++)
        {
            var template = new SotpObjectTemplate
            {
                Id = reader.ReadInt16(),
                Type = reader.ReadInt16(),
                X = reader.ReadInt16(),
                Y = reader.ReadInt16(),
                Width = reader.ReadInt16(),
                Height = reader.ReadInt16(),
                Flags = reader.ReadInt16(),
                Data = reader.ReadBytes(16) // Adjust size as per actual format
            };
            Templates.Add(template);
        }
    }
}
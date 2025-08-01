using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DarkAges.Library.IO;

public class StringTableFile
{
    public int Header { get; private set; }
    public byte[] RawData { get; private set; }
    public byte[] ProcessedData { get; private set; }
    public List<string> Strings { get; private set; } = [];
    public int StringCount { get; private set; }

    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"String table file not found: {filePath}");

        RawData = File.ReadAllBytes(filePath);
        ParseData();
    }

    public void LoadFromData(byte[] data)
    {
        RawData = data;
        ParseData();
    }

    private void ParseData()
    {
        if (RawData.Length < 4)
            throw new InvalidDataException("SOTP.DAT file too small");

        using var stream = new MemoryStream(RawData);
        using var reader = new BinaryReader(stream);

        // Read header/version (4 bytes) as seen in sub_5CF4F0
        Header = reader.ReadInt32();

        // Calculate data size after header
        var dataSize = RawData.Length - 4;
        ProcessedData = new byte[dataSize];

        // Copy data after header
        Array.Copy(RawData, 4, ProcessedData, 0, dataSize);

        // Apply the bit masking logic from sub_5CF4F0
        // The disassembly shows: v5[104][i] &= 0xFu;
        for (var i = 0; i < ProcessedData.Length; i++)
        {
            ProcessedData[i] &= 0x0F; // Mask with 0x0F
        }

        // Parse strings from processed data
        ParseStrings();
    }

    private void ParseStrings()
    {
        Strings.Clear();
        var currentString = new StringBuilder();
        var stringIndex = 0;

        for (var i = 0; i < ProcessedData.Length; i++)
        {
            var b = ProcessedData[i];
                
            // Check for null terminator or end of data
            if (b == 0 || i == ProcessedData.Length - 1)
            {
                if (currentString.Length > 0)
                {
                    Strings.Add(currentString.ToString());
                    currentString.Clear();
                    stringIndex++;
                }
            }
            else if (b >= 32 && b <= 126) // Printable ASCII range
            {
                currentString.Append((char)b);
            }
            // Skip non-printable characters
        }

        StringCount = Strings.Count;
    }

    public string GetString(int index)
    {
        if (index >= 0 && index < Strings.Count)
            return Strings[index];
        return string.Empty;
    }

    public int FindString(string searchString)
    {
        return Strings.FindIndex(s => s.Equals(searchString, StringComparison.OrdinalIgnoreCase));
    }

    public void SaveToFile(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        // Write header
        writer.Write(Header);

        // Write processed data
        writer.Write(ProcessedData);
    }
}
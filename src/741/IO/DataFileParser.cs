using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DarkAges.Library.IO;

public class DataFileParser
{
    public static DataFileInfo ParseDataFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Data file not found: {filePath}");

        var fileName = Path.GetFileName(filePath).ToUpper();
        var rawData = File.ReadAllBytes(filePath);

        switch (fileName)
        {
        case "SOTP.DAT":
            return ParseSotpFile(rawData);
        case "LEGEND.DAT":
            return ParseLegendFile(rawData);
        case "SEO.DAT":
            return ParseSeoFile(rawData);
        case "MISC.DAT":
            return ParseMiscFile(rawData);
        case "SETOA.DAT":
            return ParseSetoaFile(rawData);
        case "NATIONAL.DAT":
            return ParseNationalFile(rawData);
        case "IA.DAT":
            return ParseIaFile(rawData);
        case "HADES.DAT":
            return ParseHadesFile(rawData);
        case "ROH.DAT":
            return ParseRohFile(rawData);
        default:
            if (fileName.StartsWith("KHAN"))
            {
                return ParseKhanFile(fileName, rawData);
            }
            return ParseGenericFile(fileName, rawData);
        }
    }

    private static DataFileInfo ParseSotpFile(byte[] data)
    {
        // Based on sub_5CF4F0 and sub_5CF3B0 analysis
        var info = new DataFileInfo
        {
            FileType = "SOTP",
            FileName = "SOTP.DAT",
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        if (data.Length < 4) return info;

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        // Read header/version (4 bytes)
        info.Header = reader.ReadInt32();
            
        // Process string data based on disassembly logic
        var processedData = new byte[data.Length - 4];
        Array.Copy(data, 4, processedData, 0, processedData.Length);
            
        // Apply the bit masking logic from sub_5CF4F0
        for (var i = 0; i < processedData.Length; i++)
        {
            processedData[i] &= 0x0F; // Mask with 0x0F
        }

        info.ProcessedData = processedData;
        info.StringTable = ParseStringTable(processedData);
        info.StringCount = info.StringTable.Count;

        return info;
    }

    private static DataFileInfo ParseLegendFile(byte[] data)
    {
        // Main data file - contains core game data
        var info = new DataFileInfo
        {
            FileType = "LEGEND",
            FileName = "Legend.dat",
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse header structure
        if (data.Length >= 8)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
            info.Version = reader.ReadInt32();
        }

        return info;
    }

    private static DataFileInfo ParseSeoFile(byte[] data)
    {
        var info = new DataFileInfo
        {
            FileType = "SEO",
            FileName = "seo.dat",
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse SEO data structure
        if (data.Length >= 4)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
        }

        return info;
    }

    private static DataFileInfo ParseMiscFile(byte[] data)
    {
        var info = new DataFileInfo
        {
            FileType = "MISC",
            FileName = "misc.dat",
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse misc data structure
        if (data.Length >= 4)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
        }

        return info;
    }

    private static DataFileInfo ParseSetoaFile(byte[] data)
    {
        var info = new DataFileInfo
        {
            FileType = "SETOA",
            FileName = "setoa.dat",
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse setoa data structure
        if (data.Length >= 4)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
        }

        return info;
    }

    private static DataFileInfo ParseNationalFile(byte[] data)
    {
        var info = new DataFileInfo
        {
            FileType = "NATIONAL",
            FileName = "national.dat",
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse national data structure
        if (data.Length >= 4)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
        }

        return info;
    }

    private static DataFileInfo ParseIaFile(byte[] data)
    {
        var info = new DataFileInfo
        {
            FileType = "IA",
            FileName = "ia.dat",
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse IA data structure
        if (data.Length >= 4)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
        }

        return info;
    }

    private static DataFileInfo ParseHadesFile(byte[] data)
    {
        var info = new DataFileInfo
        {
            FileType = "HADES",
            FileName = "hades.dat",
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse hades data structure
        if (data.Length >= 4)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
        }

        return info;
    }

    private static DataFileInfo ParseRohFile(byte[] data)
    {
        var info = new DataFileInfo
        {
            FileType = "ROH",
            FileName = "roh.dat",
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse roh data structure
        if (data.Length >= 4)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
        }

        return info;
    }

    private static DataFileInfo ParseKhanFile(string fileName, byte[] data)
    {
        var info = new DataFileInfo
        {
            FileType = "KHAN",
            FileName = fileName,
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse khan data structure
        if (data.Length >= 4)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
        }

        return info;
    }

    private static DataFileInfo ParseGenericFile(string fileName, byte[] data)
    {
        var info = new DataFileInfo
        {
            FileType = "GENERIC",
            FileName = fileName,
            RawSize = data.Length,
            LoadedAt = DateTime.Now
        };

        // Parse generic data structure
        if (data.Length >= 4)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
                
            info.Header = reader.ReadInt32();
        }

        return info;
    }

    private static List<string> ParseStringTable(byte[] data)
    {
        var strings = new List<string>();
        var currentString = new StringBuilder();
            
        for (var i = 0; i < data.Length; i++)
        {
            var b = data[i];
            if (b == 0) // Null terminator
            {
                if (currentString.Length > 0)
                {
                    strings.Add(currentString.ToString());
                    currentString.Clear();
                }
            }
            else if (b >= 32 && b <= 126) // Printable ASCII
            {
                currentString.Append((char)b);
            }
        }
            
        // Add final string if any
        if (currentString.Length > 0)
        {
            strings.Add(currentString.ToString());
        }
            
        return strings;
    }
}
using System;
using System.Collections.Generic;

namespace DarkAges.Library.Core;

public static class StringTable
{
    private static readonly Dictionary<string, string> _strings = new Dictionary<string, string>();

    public static string GetString(string key)
    {
        return _strings.TryGetValue(key, out var value) ? value : key;
    }

    public static void SetString(string key, string value)
    {
        _strings[key] = value;
    }

    public static bool ContainsKey(string key)
    {
        return _strings.ContainsKey(key);
    }

    public static void Clear()
    {
        _strings.Clear();
    }

    public static void LoadFromFile(string filePath)
    {
        try
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split(['='], 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Replace("\\n", "\n");
                    SetString(key, value);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading string table from {filePath}: {ex.Message}");
        }
    }
}
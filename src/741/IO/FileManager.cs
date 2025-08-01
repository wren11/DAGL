using System;
using System.IO;
using System.Collections.Generic;

namespace DarkAges.Library.IO;

public class FileManager
{
    private static FileManager? _instance;
    public static FileManager Instance => _instance ??= new FileManager();

    private readonly Dictionary<string, byte[]> _fileCache = new Dictionary<string, byte[]>();
    private readonly string _basePath;

    private FileManager()
    {
        _basePath = AppDomain.CurrentDomain.BaseDirectory;
    }

    public bool FileExists(string fileName)
    {
        var fullPath = Path.Combine(_basePath, fileName);
        return File.Exists(fullPath);
    }

    public byte[]? ReadFile(string fileName)
    {
        if (_fileCache.TryGetValue(fileName, out var cachedData))
            return cachedData;

        var fullPath = Path.Combine(_basePath, fileName);
        if (!File.Exists(fullPath))
            return null;

        try
        {
            var data = File.ReadAllBytes(fullPath);
            _fileCache[fileName] = data;
            return data;
        }
        catch
        {
            return null;
        }
    }

    public byte[]? LoadFile(string fileName)
    {
        return ReadFile(fileName);
    }

    public bool WriteFile(string fileName, byte[] data)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, fileName);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(fullPath, data);
            _fileCache[fileName] = data;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void ClearCache()
    {
        _fileCache.Clear();
    }

    public void RemoveFromCache(string fileName)
    {
        _fileCache.Remove(fileName);
    }
}
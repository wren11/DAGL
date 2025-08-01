using System;
using System.Collections.Generic;
using System.Drawing;

namespace DarkAges.Library.IO;

public class ResourceManager
{
    private static ResourceManager? _instance;
    public static ResourceManager Instance => _instance ??= new ResourceManager();

    private readonly Dictionary<string, object> _resources = new Dictionary<string, object>();
    private readonly FileManager _fileManager;

    private ResourceManager()
    {
        _fileManager = FileManager.Instance;
    }

    public T? GetResource<T>(string resourceName) where T : class
    {
        if (_resources.TryGetValue(resourceName, out var resource))
        {
            return resource as T;
        }
        return null;
    }

    public void AddResource<T>(string resourceName, T resource) where T : class
    {
        _resources[resourceName] = resource;
    }

    public bool HasResource(string resourceName)
    {
        return _resources.ContainsKey(resourceName);
    }

    public void RemoveResource(string resourceName)
    {
        _resources.Remove(resourceName);
    }

    public void ClearResources()
    {
        _resources.Clear();
    }

    public Image? LoadImage(string fileName)
    {
        var data = _fileManager.ReadFile(fileName);
        if (data == null) return null;

        try
        {
            using var stream = new System.IO.MemoryStream(data);
            return Image.FromStream(stream);
        }
        catch
        {
            return null;
        }
    }

    public byte[]? LoadData(string fileName)
    {
        return _fileManager.ReadFile(fileName);
    }
}
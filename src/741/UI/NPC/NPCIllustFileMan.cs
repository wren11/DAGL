using System;
using System.Collections.Generic;
using System.IO;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI.NPC;

public class NPCIllustFileMan
{
    private readonly Dictionary<int, ImagePane> _illustrationCache = new Dictionary<int, ImagePane>();
    private readonly Dictionary<int, string> _illustrationPaths = new Dictionary<int, string>();
    private string _basePath = "data/illustrations/";
    private int _maxCacheSize = 50;
    private readonly Queue<int> _cacheOrder = new Queue<int>();

    public event EventHandler<IllustrationEventArgs> IllustrationLoaded = delegate { };
    public event EventHandler<IllustrationEventArgs> IllustrationUnloaded = delegate { };

    public NPCIllustFileMan()
    {
        InitializeIllustrationPaths();
    }

    private void InitializeIllustrationPaths()
    {
        try
        {
            var configFile = Path.Combine(_basePath, "illustrations.dat");
            if (File.Exists(configFile))
            {
                LoadIllustrationConfig(configFile);
            }
            else
            {
                LoadDefaultIllustrations();
            }
        }
        catch
        {
            LoadDefaultIllustrations();
        }
    }

    private void LoadIllustrationConfig(string configFile)
    {
        using var reader = new StreamReader(configFile);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var parts = line.Split('=');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0].Trim(), out var id))
                {
                    var path = parts[1].Trim();
                    _illustrationPaths[id] = path;
                }
            }
        }
    }

    private void LoadDefaultIllustrations()
    {
        var defaultIllustrations = new Dictionary<int, string>
        {
            { 1, "npc_001.epf" },
            { 2, "npc_002.epf" },
            { 3, "npc_003.epf" },
            { 4, "npc_004.epf" },
            { 5, "npc_005.epf" },
            { 10, "merchant_001.epf" },
            { 11, "merchant_002.epf" },
            { 20, "quest_001.epf" },
            { 21, "quest_002.epf" },
            { 30, "trainer_001.epf" },
            { 31, "trainer_002.epf" }
        };

        foreach (var kvp in defaultIllustrations)
        {
            _illustrationPaths[kvp.Key] = kvp.Value;
        }
    }

    public ImagePane? GetIllustration(int npcId)
    {
        if (_illustrationCache.TryGetValue(npcId, out var cachedImage))
        {
            UpdateCacheOrder(npcId);
            return cachedImage;
        }

        var image = LoadIllustration(npcId);
        if (image != null)
        {
            AddToCache(npcId, image);
            IllustrationLoaded?.Invoke(this, new IllustrationEventArgs(npcId, image));
        }

        return image;
    }

    private ImagePane? LoadIllustration(int npcId)
    {
        if (!_illustrationPaths.TryGetValue(npcId, out var relativePath))
        {
            return LoadDefaultIllustration();
        }

        try
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            if (File.Exists(fullPath))
            {
                return LoadIllustrationFromFile(fullPath);
            }
            else
            {
                return LoadDefaultIllustration();
            }
        }
        catch
        {
            return LoadDefaultIllustration();
        }
    }

    private ImagePane LoadIllustrationFromFile(string filePath)
    {
        try
        {
            var extension = Path.GetExtension(filePath).ToLower();
                
            switch (extension)
            {
            case ".epf":
                return LoadEpfIllustration(filePath);
            case ".pcx":
                return LoadPcxIllustration(filePath);
            case ".bmp":
            case ".png":
            case ".jpg":
            case ".jpeg":
                return LoadStandardImage(filePath);
            default:
                return LoadDefaultIllustration();
            }
        }
        catch
        {
            return LoadDefaultIllustration();
        }
    }

    private ImagePane LoadEpfIllustration(string filePath)
    {
        try
        {
            var epfArchive = new EpfArchive(filePath);
            if (epfArchive.Files.Count > 0)
            {
                var firstEntry = epfArchive.Files[0];
                var imageData = firstEntry.GetData();
                    
                if (imageData != null)
                {
                    var indexedImage = new IndexedImage(64, 64, imageData);
                    var imagePane = new ImagePane();
                    imagePane.SetImage(indexedImage, new Palette());
                    return imagePane;
                }
            }
        }
        catch
        {
        }

        return LoadDefaultIllustration();
    }

    private ImagePane LoadPcxIllustration(string filePath)
    {
        try
        {
            var indexedImage = DarkAges.Library.IO.PcxReader.Load(filePath);
            if (indexedImage != null)
            {
                var imagePane = new ImagePane();
                imagePane.SetImage(indexedImage, new Palette());
                return imagePane;
            }
        }
        catch
        {
        }

        return LoadDefaultIllustration();
    }

    private ImagePane LoadStandardImage(string filePath)
    {
        try
        {
            var image = ImageLoader.LoadImage(filePath);
            var imagePane = new ImagePane();
            // Convert System.Drawing.Image to IndexedImage if needed
            // For now, return a placeholder
            return CreatePlaceholderIllustration();
        }
        catch
        {
        }

        return LoadDefaultIllustration();
    }

    private ImagePane LoadDefaultIllustration()
    {
        try
        {
            var defaultPath = Path.Combine(_basePath, "default.epf");
            if (File.Exists(defaultPath))
            {
                return LoadEpfIllustration(defaultPath);
            }
        }
        catch
        {
        }

        return CreatePlaceholderIllustration();
    }

    private ImagePane CreatePlaceholderIllustration()
    {
        var placeholderImage = new IndexedImage(64, 64);
            
        // Create a simple NPC silhouette pattern
        var pixels = new byte[64 * 64];
            
        // Fill with a default background color
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = 0; // Background
            
        // Draw a simple humanoid figure
        DrawHumanoidSilhouette(pixels, 64, 64);
            
        placeholderImage.SetPixelData(pixels);
            
        var imagePane = new ImagePane();
        imagePane.SetImage(placeholderImage, null);
        return imagePane;
    }

    private void DrawHumanoidSilhouette(byte[] pixels, int width, int height)
    {
        var centerX = width / 2;
        var centerY = height / 2;
            
        // Head (circle)
        DrawCircle(pixels, width, height, centerX, centerY - 20, 8, 1);
            
        // Body (rectangle)
        DrawRectangle(pixels, width, height, centerX - 6, centerY - 10, 12, 20, 1);
            
        // Arms
        DrawRectangle(pixels, width, height, centerX - 15, centerY - 8, 8, 3, 1); // Left arm
        DrawRectangle(pixels, width, height, centerX + 7, centerY - 8, 8, 3, 1);  // Right arm
            
        // Legs
        DrawRectangle(pixels, width, height, centerX - 4, centerY + 10, 3, 15, 1); // Left leg
        DrawRectangle(pixels, width, height, centerX + 1, centerY + 10, 3, 15, 1); // Right leg
    }

    private void DrawCircle(byte[] pixels, int width, int height, int centerX, int centerY, int radius, byte color)
    {
        for (var y = -radius; y <= radius; y++)
        {
            for (var x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    var pixelX = centerX + x;
                    var pixelY = centerY + y;
                    if (pixelX >= 0 && pixelX < width && pixelY >= 0 && pixelY < height)
                    {
                        pixels[pixelY * width + pixelX] = color;
                    }
                }
            }
        }
    }

    private void DrawRectangle(byte[] pixels, int width, int height, int x, int y, int rectWidth, int rectHeight, byte color)
    {
        for (var dy = 0; dy < rectHeight; dy++)
        {
            for (var dx = 0; dx < rectWidth; dx++)
            {
                var pixelX = x + dx;
                var pixelY = y + dy;
                if (pixelX >= 0 && pixelX < width && pixelY >= 0 && pixelY < height)
                {
                    pixels[pixelY * width + pixelX] = color;
                }
            }
        }
    }

    private void AddToCache(int npcId, ImagePane image)
    {
        if (_illustrationCache.Count >= _maxCacheSize)
        {
            var oldestId = _cacheOrder.Dequeue();
            if (_illustrationCache.TryGetValue(oldestId, out var oldImage))
            {
                _illustrationCache.Remove(oldestId);
                oldImage.Dispose();
                IllustrationUnloaded?.Invoke(this, new IllustrationEventArgs(oldestId, null!));
            }
        }

        _illustrationCache[npcId] = image;
        _cacheOrder.Enqueue(npcId);
    }

    private void UpdateCacheOrder(int npcId)
    {
        var tempQueue = new Queue<int>();
        var found = false;

        while (_cacheOrder.Count > 0)
        {
            var id = _cacheOrder.Dequeue();
            if (id != npcId)
            {
                tempQueue.Enqueue(id);
            }
            else
            {
                found = true;
            }
        }

        while (tempQueue.Count > 0)
        {
            _cacheOrder.Enqueue(tempQueue.Dequeue());
        }

        if (found)
        {
            _cacheOrder.Enqueue(npcId);
        }
    }

    public void PreloadIllustration(int npcId)
    {
        if (!_illustrationCache.ContainsKey(npcId))
        {
            GetIllustration(npcId);
        }
    }

    public void PreloadIllustrations(int[] npcIds)
    {
        foreach (var npcId in npcIds)
        {
            PreloadIllustration(npcId);
        }
    }

    public void UnloadIllustration(int npcId)
    {
        if (_illustrationCache.TryGetValue(npcId, out var image))
        {
            _illustrationCache.Remove(npcId);
            image.Dispose();
            IllustrationUnloaded?.Invoke(this, new IllustrationEventArgs(npcId, null!));
        }
    }

    public void ClearCache()
    {
        foreach (var kvp in _illustrationCache)
        {
            kvp.Value.Dispose();
            IllustrationUnloaded?.Invoke(this, new IllustrationEventArgs(kvp.Key, null!));
        }

        _illustrationCache.Clear();
        _cacheOrder.Clear();
    }

    public bool HasIllustration(int npcId)
    {
        return _illustrationCache.ContainsKey(npcId) || _illustrationPaths.ContainsKey(npcId);
    }

    public void AddIllustrationPath(int npcId, string path)
    {
        _illustrationPaths[npcId] = path;
    }

    public void RemoveIllustrationPath(int npcId)
    {
        _illustrationPaths.Remove(npcId);
        UnloadIllustration(npcId);
    }

    public List<int> GetLoadedIllustrationIds()
    {
        return [.._illustrationCache.Keys];
    }

    public List<int> GetAvailableIllustrationIds()
    {
        return [.._illustrationPaths.Keys];
    }

    public int GetCacheSize()
    {
        return _illustrationCache.Count;
    }

    public void SetMaxCacheSize(int maxSize)
    {
        _maxCacheSize = Math.Max(1, maxSize);
            
        while (_illustrationCache.Count > _maxCacheSize)
        {
            var oldestId = _cacheOrder.Dequeue();
            UnloadIllustration(oldestId);
        }
    }

    public void SetBasePath(string basePath)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
    }

    public void ReloadIllustration(int npcId)
    {
        UnloadIllustration(npcId);
        GetIllustration(npcId);
    }

    public void ReloadAllIllustrations()
    {
        var loadedIds = GetLoadedIllustrationIds();
        ClearCache();
            
        foreach (var npcId in loadedIds)
        {
            GetIllustration(npcId);
        }
    }

    public void Dispose()
    {
        ClearCache();
    }
}
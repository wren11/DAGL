using DarkAges.Library.Common.DataStructures;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace DarkAges.Library.GameLogic;

public class HumanImageCache(int maxCacheSize = 1000)
{
    public class CacheEntry
    {
        public int Id { get; set; }
        public IndexedImage Image { get; set; }
        public List<int> FrameData { get; set; }
        public DateTime LastAccessed { get; set; }
        public int AccessCount { get; set; }
    }

    private readonly RedBlackTree<int, CacheEntry> _cache = new();
    private readonly List<IndexedImage> _images = [];
    private readonly object _cacheLock = new object();

    public void LoadCache(string fileName)
    {
        lock (_cacheLock)
        {
            try
            {
                var extension = Path.GetExtension(fileName).ToLower();
                    
                switch (extension)
                {
                case ".epf":
                    LoadEpfFile(fileName);
                    break;
                case ".alb":
                    LoadAlbFile(fileName);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported file format: {extension}");
                }
            }
            catch (Exception ex)
            {
                // Log the error and continue with empty cache
                Console.WriteLine($"Failed to load human image cache from {fileName}: {ex.Message}");
            }
        }
    }

    private void LoadEpfFile(string fileName)
    {
        var epfFile = new EpfArchive(fileName);
            
        for (var i = 0; i < epfFile.Files.Count; i++)
        {
            var entry = epfFile.Files[i];
            try
            {
                var imageData = entry.GetData();
                if (imageData != null)
                {
                    var image = ImageLoader.LoadImageFromBytes(imageData);
                    if (image != null)
                    {
                        var cacheEntry = new CacheEntry
                        {
                            Id = i,
                            Image = image,
                            FrameData = [],
                            LastAccessed = DateTime.Now,
                            AccessCount = 0
                        };

                        _cache.Add(i, cacheEntry);
                        _images.Add(image);
                    }
                }
            }
            catch
            {
                // Skip corrupted entries
                continue;
            }
        }
    }

    private void LoadAlbFile(string fileName)
    {
        var albumFile = new AlbumFile(fileName);
            
        for (var i = 0; i < albumFile.Entries.Count; i++)
        {
            var entry = albumFile.Entries[i];
            try
            {
                var imageData = albumFile.GetImageData(entry);
                if (imageData != null)
                {
                    var image = ImageLoader.LoadImageFromBytes(imageData);
                    if (image != null)
                    {
                        var cacheEntry = new CacheEntry
                        {
                            Id = i,
                            Image = image,
                            FrameData = [],
                            LastAccessed = DateTime.Now,
                            AccessCount = 0
                        };

                        _cache.Add(i, cacheEntry);
                        _images.Add(image);
                    }
                }
            }
            catch
            {
                // Skip corrupted entries
                continue;
            }
        }
    }

    public IndexedImage GetImage(int id)
    {
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(id, out var entry))
            {
                // Update access statistics
                entry.LastAccessed = DateTime.Now;
                entry.AccessCount++;
                    
                return entry.Image;
            }
            return null;
        }
    }

    public IndexedImage GetImageWithFallback(int id, string fallbackFileName = null)
    {
        var image = GetImage(id);
        if (image != null)
            return image;

        // Try to load from fallback file
        if (!string.IsNullOrEmpty(fallbackFileName))
        {
            try
            {
                LoadCache(fallbackFileName);
                return GetImage(id);
            }
            catch
            {
                // Fallback failed
            }
        }

        return null;
    }

    public void PreloadImages(int[] ids)
    {
        foreach (var id in ids)
        {
            GetImage(id); // This will trigger loading if not cached
        }
    }

    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _cache.Clear();
            _images.Clear();
        }
    }

    public void RemoveFromCache(int id)
    {
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(id, out var entry))
            {
                _images.Remove(entry.Image);
                _cache.Remove(id);
            }
        }
    }

    public void OptimizeCache()
    {
        lock (_cacheLock)
        {
            if (_cache.Count <= maxCacheSize)
                return;

            // Remove least recently used entries
            var entries = new List<CacheEntry>();
            foreach (KeyValuePair<int, CacheEntry> kvp in _cache)
            {
                entries.Add(kvp.Value);
            }

            // Sort by last accessed time and access count
            entries.Sort((a, b) => 
            {
                var timeCompare = a.LastAccessed.CompareTo(b.LastAccessed);
                if (timeCompare != 0)
                    return timeCompare;
                return a.AccessCount.CompareTo(b.AccessCount);
            });

            // Remove oldest entries
            int toRemove = _cache.Count - maxCacheSize;
            for (var i = 0; i < toRemove; i++)
            {
                var entry = entries[i];
                _images.Remove(entry.Image);
                _cache.Remove(entry.Id);
            }
        }
    }

    public int GetCacheSize()
    {
        lock (_cacheLock)
        {
            return _cache.Count;
        }
    }

    public List<int> GetCachedIds()
    {
        lock (_cacheLock)
        {
            var ids = new List<int>();
            foreach (KeyValuePair<int, CacheEntry> kvp in _cache)
            {
                ids.Add(kvp.Key);
            }
            return ids;
        }
    }

    public bool IsCached(int id)
    {
        lock (_cacheLock)
        {
            return _cache.ContainsKey(id);
        }
    }

    public IndexedImage GetHumanImage(short gender, short angle)
    {
        // Calculate image ID based on gender and angle
        var imageId = (gender * 8) + angle;
        return GetImage(imageId);
    }

    public IndexedImage GetHairImage(short gender, short hairStyle)
    {
        // Calculate hair image ID based on gender and hair style
        var imageId = 1000 + (gender * 10) + hairStyle;
        return GetImage(imageId);
    }

    public void Dispose()
    {
        lock (_cacheLock)
        {
            foreach (var image in _images)
            {
                image?.Dispose();
            }
            _images.Clear();
            _cache.Clear();
        }
    }
}
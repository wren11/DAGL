using DarkAges.Library.Common.DataStructures;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DarkAges.Library.GameLogic;

public class DamageEffectImageCache
{
    public class EffectEntry
    {
        public int Id { get; set; }
        public IndexedImage Image { get; set; }
        public int FrameCount { get; set; }
        public float Duration { get; set; }
        public bool IsLooping { get; set; }
    }

    private readonly RedBlackTree<int, EffectEntry> _images = new();
    private readonly Dictionary<int, List<IndexedImage>> _animationFrames = new();
    private readonly object _cacheLock = new object();

    public void LoadImages(string fileName)
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
                case ".spf":
                    LoadSpfFile(fileName);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported file format: {extension}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load damage effect images from {fileName}: {ex.Message}");
            }
        }
    }

    private void LoadEpfFile(string fileName)
    {
        var epfFile = new EpfArchive(fileName);
            
        foreach (var entry in epfFile.Entries)
        {
            try
            {
                var image = ImageLoader.LoadImageFromBytes(entry.Data);
                if (image != null)
                {
                    var effectEntry = new EffectEntry
                    {
                        Id = entry.Id,
                        Image = image,
                        FrameCount = 1,
                        Duration = 1.0f,
                        IsLooping = false
                    };

                    _images.Insert(entry.Id, effectEntry);
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
            
        foreach (var entry in albumFile.Entries)
        {
            try
            {
                var image = ImageLoader.LoadImageFromBytes(entry.Data);
                if (image != null)
                {
                    var effectEntry = new EffectEntry
                    {
                        Id = entry.Id,
                        Image = image,
                        FrameCount = 1,
                        Duration = 1.0f,
                        IsLooping = false
                    };

                    _images.Insert(entry.Id, effectEntry);
                }
            }
            catch
            {
                // Skip corrupted entries
                continue;
            }
        }
    }

    private void LoadSpfFile(string fileName)
    {
        var spfFile = new SpfFile(fileName);
            
        for (short i = 0; i < spfFile.FrameCount; i++)
        {
            try
            {
                var frame = spfFile.GetFrame(i);
                if (frame != null)
                {
                    var image = ImageLoader.LoadImageFromIndexedImage(frame);
                    if (image != null)
                    {
                        var effectEntry = new EffectEntry
                        {
                            Id = i,
                            Image = image,
                            FrameCount = spfFile.FrameCount,
                            Duration = 2.0f, // Default 2 seconds for SPF animations
                            IsLooping = true
                        };

                        _images.Insert(i, effectEntry);
                    }
                }
            }
            catch
            {
                // Skip corrupted frames
                continue;
            }
        }
    }

    public IndexedImage GetImage(int index)
    {
        lock (_cacheLock)
        {
            if (_images.TryGetValue(index, out var entry))
            {
                return entry.Image;
            }
            return null!;
        }
    }

    public EffectEntry GetEffect(int index)
    {
        lock (_cacheLock)
        {
            if (_images.TryGetValue(index, out var entry))
            {
                return entry;
            }
            return null;
        }
    }

    public List<IndexedImage> GetAnimationFrames(int baseIndex)
    {
        lock (_cacheLock)
        {
            if (_animationFrames.TryGetValue(baseIndex, out var frames))
            {
                return [..frames];
            }

            // Try to build animation frames from sequential images
            var frameList = new List<IndexedImage>();
            var currentIndex = baseIndex;
                
            while (_images.TryGetValue(currentIndex, out var entry))
            {
                frameList.Add(entry.Image);
                currentIndex++;
            }

            if (frameList.Count > 0)
            {
                _animationFrames[baseIndex] = frameList;
                return frameList;
            }

            return null;
        }
    }

    public IndexedImage GetAnimationFrame(int baseIndex, int frameNumber)
    {
        var frames = GetAnimationFrames(baseIndex);
        if (frames != null && frameNumber >= 0 && frameNumber < frames.Count)
        {
            return frames[frameNumber];
        }
        return null;
    }

    public void PreloadEffect(int index)
    {
        GetImage(index); // This will trigger loading if not cached
    }

    public void PreloadEffects(int[] indices)
    {
        foreach (var index in indices)
        {
            PreloadEffect(index);
        }
    }

    public void ClearCache()
    {
        lock (_cacheLock)
        {
            foreach (KeyValuePair<int, EffectEntry> kvp in _images)
            {
                kvp.Value.Image?.Dispose();
            }
            _images.Clear();
            _animationFrames.Clear();
        }
    }

    public void RemoveEffect(int index)
    {
        lock (_cacheLock)
        {
            if (_images.TryGetValue(index, out var entry))
            {
                entry.Image?.Dispose();
                _images.Remove(index);
            }

            // Remove from animation frames if present
            if (_animationFrames.ContainsKey(index))
            {
                _animationFrames.Remove(index);
            }
        }
    }

    public int GetEffectCount()
    {
        lock (_cacheLock)
        {
            return _images.Count;
        }
    }

    public List<int> GetEffectIds()
    {
        lock (_cacheLock)
        {
            var ids = new List<int>();
            foreach (KeyValuePair<int, EffectEntry> kvp in _images)
            {
                ids.Add(kvp.Key);
            }
            return ids;
        }
    }

    public bool HasEffect(int index)
    {
        lock (_cacheLock)
        {
            return _images.ContainsKey(index);
        }
    }

    public void SetEffectProperties(int index, float duration, bool isLooping)
    {
        lock (_cacheLock)
        {
            if (_images.TryGetValue(index, out var entry))
            {
                entry.Duration = duration;
                entry.IsLooping = isLooping;
            }
        }
    }

    public void Dispose()
    {
        lock (_cacheLock)
        {
            foreach (KeyValuePair<int, EffectEntry> entry in _images)
            {
                entry.Value.Image?.Dispose();
            }
            _images.Clear();
            _animationFrames.Clear();
        }
    }
}
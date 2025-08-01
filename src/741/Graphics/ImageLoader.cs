using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DarkAges.Library.IO;
using DarkAges.Library.GameLogic;

namespace DarkAges.Library.Graphics;

public static class ImageLoader
{
    private static readonly Dictionary<string, IndexedImage> _imageCache = new();
    private static readonly Dictionary<(ItemType, ushort), FrameInfo> _itemIconCache = new();
        
    public static IndexedImage? LoadImageFromBytes(byte[] imageData)
    {
        try
        {
            using var stream = new MemoryStream(imageData);
            using var bitmap = new Bitmap(stream);
            var width = bitmap.Width;
            var height = bitmap.Height;
            var pixelData = new byte[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    // Convert to grayscale index (simple stub, real implementation may differ)
                    pixelData[y * width + x] = (byte)((color.R + color.G + color.B) / 3);
                }
            }
            return new IndexedImage(width, height, pixelData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load image from bytes: {ex.Message}");
            return null;
        }
    }

    public static IndexedImage? LoadImageFromFile(string fileName)
    {
        try
        {
            // Check cache first
            if (_imageCache.TryGetValue(fileName, out var cachedImage))
                return cachedImage;

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Image file not found: {fileName}");
                return null;
            }
                
            var bytes = File.ReadAllBytes(fileName);
            var image = LoadImageFromBytes(bytes);
                
            // Cache the loaded image
            if (image != null)
            {
                _imageCache[fileName] = image;
            }
                
            return image;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load image from file {fileName}: {ex.Message}");
            return null;
        }
    }

    // Loads an image from a file path (for compatibility)
    public static IndexedImage? LoadImage(string fileName)
    {
        return LoadImageFromFile(fileName);
    }

    public static FrameInfo? GetItemIcon(ItemType itemType, ushort itemId)
    {
        var key = (itemType, itemId);
            
        // Check cache first
        if (_itemIconCache.TryGetValue(key, out var cachedFrame))
            return cachedFrame;

        try
        {
            // Load from EPF archive
            var archiveNumber = (itemId / 266) + 1;
            var archiveName = $"item{archiveNumber:D3}.epf";
            var archivePath = Path.Combine("Data", archiveName);

            if (!File.Exists(archivePath))
            {
                Console.WriteLine($"Item archive not found: {archivePath}");
                return null;
            }

            var archive = new EpfArchive(archivePath);
            var frameIndex = itemId % 266;
            var frame = archive.GetFrame(frameIndex);

            if (frame != null)
            {
                _itemIconCache[key] = frame;
            }

            return frame;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load item icon {itemType}:{itemId}: {ex.Message}");
            return null;
        }
    }

    public static IndexedImage? LoadCharacterImage(short gender, short angle, short animationFrame = 0)
    {
        try
        {
            // Character images are stored in specific archives
            var genderPrefix = gender == 0 ? "male" : "female";
            var fileName = $"{genderPrefix}_angle{angle}_frame{animationFrame}.spf";
            var filePath = Path.Combine("Data", "Characters", fileName);

            return LoadImageFromFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load character image: {ex.Message}");
            return null;
        }
    }

    public static IndexedImage? LoadHairImage(short gender, short hairStyle)
    {
        try
        {
            var genderPrefix = gender == 0 ? "male" : "female";
            var fileName = $"hair_{genderPrefix}_{hairStyle:D2}.spf";
            var filePath = Path.Combine("Data", "Hair", fileName);

            return LoadImageFromFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load hair image: {ex.Message}");
            return null;
        }
    }

    public static IndexedImage? LoadEquipmentImage(EquipSlot slot, int itemId, short gender = 0)
    {
        try
        {
            var slotName = slot.ToString().ToLower();
            var genderPrefix = gender == 0 ? "male" : "female";
            var fileName = $"{slotName}_{genderPrefix}_{itemId:D4}.spf";
            var filePath = Path.Combine("Data", "Equipment", fileName);

            return LoadImageFromFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load equipment image: {ex.Message}");
            return null;
        }
    }

    public static IndexedImage? LoadMapTile(int tileId)
    {
        try
        {
            var fileName = $"tile_{tileId:D4}.spf";
            var filePath = Path.Combine("Data", "Tiles", fileName);

            return LoadImageFromFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load map tile: {ex.Message}");
            return null;
        }
    }

    public static IndexedImage? LoadEffectImage(int effectId, int frameIndex = 0)
    {
        try
        {
            var fileName = $"effect_{effectId:D3}_frame_{frameIndex:D2}.spf";
            var filePath = Path.Combine("Data", "Effects", fileName);

            return LoadImageFromFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load effect image: {ex.Message}");
            return null;
        }
    }

    public static IndexedImage? LoadUIImage(string imageName)
    {
        try
        {
            var fileName = $"{imageName}.bmp";
            var filePath = Path.Combine("Data", "UI", fileName);

            return LoadImageFromFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load UI image: {ex.Message}");
            return null;
        }
    }

    public static void ClearCache()
    {
        foreach (var image in _imageCache.Values)
        {
            image?.Dispose();
        }
        _imageCache.Clear();
        _itemIconCache.Clear();
    }

    public static void Dispose()
    {
        ClearCache();
    }

    public static IndexedImage LoadImageFromIndexedImage(IndexedImage frame)
    {
        if (frame == null)
        {
            Console.WriteLine("Cannot load image from null IndexedImage.");
            return null;
        }
        // Create a new IndexedImage from the existing one
        var newImage = new IndexedImage(frame.Width, frame.Height, frame.PixelData);
        newImage.Palette = frame.Palette; // Copy palette if available
        return newImage;
    }

    public static IndexedImage LoadIndexedImage(string imagePath)
    {
        return LoadImageFromFile(imagePath);
    }
}
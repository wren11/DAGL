using System;
using System.IO;
using System.Runtime.InteropServices;
using DarkAges.Library.Graphics;
using System.Collections.Generic;
using DarkAges.Library.IO;

namespace DarkAges.Library.Graphics;

/// <summary>
/// Handles palette table operations for BiR (Binary Resource) files
/// </summary>
public class BiRPaletteTable : IDisposable
{
    private const int MAX_ENTRIES = 1000;
    private const int ENTRY_SIZE = 16;

    private readonly int _paletteCount;
    private readonly List<Palette> _palettes;
    private readonly FileManager _fileManager;
    private readonly ResourceManager _resourceManager;
    private readonly string _fileName;
        
    // Additional fields for table operations
    private byte[] tableData;
    private int entryCount;
    private int currentPosition;
    private bool isInitialized;

    public BiRPaletteTable(string fileName, int paletteCount)
    {
        _fileName = fileName;
        _paletteCount = paletteCount;
        _palettes = new List<Palette>(paletteCount);
        _fileManager = FileManager.Instance;
        _resourceManager = ResourceManager.Instance;
        LoadPalettes();
    }

    private void LoadPalettes()
    {
        // This method is not fully implemented in the original file,
        // so it's left as a placeholder.
        // In a real scenario, you would load palettes from the file
        // using _fileManager and _resourceManager.
    }

    public bool Initialize(string filename)
    {
        try
        {
            var data = _fileManager.LoadFile(filename);
            if (data != null && data.Length > 0)
            {
                tableData = data;
                entryCount = data.Length / ENTRY_SIZE;
                currentPosition = 0;
                isInitialized = true;
                return true;
            }
        }
        catch (Exception)
        {
            // Handle file loading errors
        }

        return false;
    }

    public bool ReadEntry(out int startFrame, out int endFrame, out int paletteIndex)
    {
        startFrame = 0;
        endFrame = 0;
        paletteIndex = 0;

        if (!isInitialized || tableData == null || currentPosition >= entryCount)
        {
            return false;
        }

        try
        {
            var offset = currentPosition * ENTRY_SIZE;
                
            using (var ms = new MemoryStream(tableData, offset, ENTRY_SIZE))
            using (var reader = new BinaryReader(ms))
            {
                startFrame = reader.ReadInt32();
                endFrame = reader.ReadInt32();
                paletteIndex = reader.ReadInt32();
                    
                // Skip padding
                reader.ReadInt32();
            }

            currentPosition++;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Reset()
    {
        currentPosition = 0;
    }

    public bool IsEndOfTable()
    {
        return !isInitialized || currentPosition >= entryCount;
    }

    public int GetEntryCount()
    {
        return entryCount;
    }

    public int GetCurrentPosition()
    {
        return currentPosition;
    }

    public void SetPosition(int position)
    {
        if (position >= 0 && position < entryCount)
        {
            currentPosition = position;
        }
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }

    public void Dispose()
    {
        tableData = null;
        isInitialized = false;
    }
}
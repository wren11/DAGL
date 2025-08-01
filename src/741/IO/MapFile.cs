using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DarkAges.Library.Common;

namespace DarkAges.Library.IO;

public class MapFile
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }
    public ushort[,] Tiles { get; private set; }
    public List<MapObject> Objects { get; private set; } = [];
    public byte[] RawData { get; private set; }

    public MapFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Map file not found: {filePath}");

        RawData = File.ReadAllBytes(filePath);
        Parse(RawData);
    }

    private void Parse(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        // Header parsing (based on chunk_005.txt and typical DA map format)
        // Map header is usually 0x40 bytes
        stream.Seek(0, SeekOrigin.Begin);
        Width = reader.ReadInt16();
        Height = reader.ReadInt16();
        TileWidth = reader.ReadInt16();
        TileHeight = reader.ReadInt16();
        // Skip unknowns (header padding)
        stream.Seek(0x40, SeekOrigin.Begin);

        // Tile data parsing (RLE or direct)
        Tiles = new ushort[Width, Height];
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                Tiles[x, y] = reader.ReadUInt16();
            }
        }

        // Object/entity parsing (if present)
        // This is a simplified version; actual format may have more fields
        if (stream.Position < stream.Length)
        {
            int objectCount = reader.ReadInt16();
            for (var i = 0; i < objectCount; i++)
            {
                var obj = new MapObject
                {
                    X = reader.ReadInt16(),
                    Y = reader.ReadInt16(),
                    Type = reader.ReadInt16(),
                    Id = reader.ReadInt16(),
                    Flags = reader.ReadInt16()
                };
                Objects.Add(obj);
            }
        }
    }
}
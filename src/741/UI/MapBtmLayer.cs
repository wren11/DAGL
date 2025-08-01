using System.Collections.Generic;
using System.IO;
using DarkAges.Library.IO;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI;

public class MapBtmLayer
{
    private readonly MapBtmImageLib _btmImageLib = new MapBtmImageLib();
    private readonly List<object> _lightList = [];
    private readonly List<object> _objectList = [];
        
    public bool IsLoaded { get; private set; } = false;
    public short Width { get; private set; }
    public short Height { get; private set; }
    public int AmbientLight { get; private set; } = -1;
    public int OutdoorLight { get; private set; } = -1;

    public bool Load(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);
            var header = reader.ReadBytes(4);
            if (header[0] != 'B' || header[1] != 'T' || header[2] != 'M' || header[3] != 0)
                return false;

            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
                    
            var lightCount = reader.ReadInt32();
            var objectCount = reader.ReadInt32();
                    
            AmbientLight = reader.ReadInt32();
            OutdoorLight = reader.ReadInt32();

            for (var i = 0; i < lightCount; i++)
            {
                var lightData = reader.ReadBytes(16);
                _lightList.Add(lightData);
            }

            for (var i = 0; i < objectCount; i++)
            {
                var objectData = reader.ReadBytes(32);
                _objectList.Add(objectData);
            }

            IsLoaded = true;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Render(SpriteBatch spriteBatch, int x, int y)
    {
        if (!IsLoaded) return;

        _btmImageLib.Render(spriteBatch, x, y, Width, Height);
    }

    public void Dispose()
    {
        _btmImageLib?.Dispose();
        _lightList.Clear();
        _objectList.Clear();
        IsLoaded = false;
    }
}
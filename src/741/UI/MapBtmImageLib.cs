using DarkAges.Library.Graphics;
using System.Collections.Generic;
using DarkAges.Library.IO;
using System.IO;

namespace DarkAges.Library.UI;

public class MapBtmImageLib
{
    private readonly List<short> _imageIds = [];
    private readonly List<ImagePane> _images = [];
    private readonly Palette _mapPalette;
    private short _imageCount;
    private short _capacity;

    public MapBtmImageLib()
    {
        _imageCount = 0;
        _capacity = 0;
            
        var palettePath = Path.Combine("Data", "map.pal");
        if (File.Exists(palettePath))
        {
            _mapPalette = PaletteManager.GetPalette("default");
        }
        else
        {
            _mapPalette = new Palette();
        }
    }

    public void LoadImage(short imageId)
    {
        if (_imageIds.Contains(imageId))
            return;

        try
        {
            var imagePath = Path.Combine("Data", "Maps", $"btm{imageId:D6}.epf");
            if (File.Exists(imagePath))
            {
                var image = ImageLoader.LoadImage(imagePath);
                if (image != null)
                {
                    var imagePane = new ImagePane();
                    imagePane.SetImage(image, _mapPalette);
                        
                    _images.Add(imagePane);
                    _imageIds.Add(imageId);
                    _imageCount++;
                }
            }
        }
        catch
        {
        }
    }

    public void Render(SpriteBatch spriteBatch, int x, int y, short width, short height)
    {
        for (var i = 0; i < _imageCount; i++)
        {
            if (i < _images.Count)
            {
                _images[i].Position = new System.Drawing.Point(x, y);
                _images[i].Size = new System.Drawing.Size(width, height);
                _images[i].Render(spriteBatch);
            }
        }
    }

    public void Dispose()
    {
        foreach (var image in _images)
        {
            image?.Dispose();
        }
        _images.Clear();
        _imageIds.Clear();
        _imageCount = 0;
        _capacity = 0;
    }
}
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;
using System;
using System.Drawing;
using System.IO;

namespace DarkAges.Library.UI;

public class LogoPane() : ImagePane(true)
{
    private bool _isLoaded = false;
    private short _logoId = 0;
    private byte[] _logoData = null;
    private byte[] _paletteData = null;
    private int _logoWidth = 0;
    private int _logoHeight = 0;
    private int _logoX = 0;
    private int _logoY = 0;
    private int _compressionType = -1;

    public bool LoadLogo(short logoId = 0)
    {
        _logoId = logoId;
            
        try
        {
            if (!LoadLogoData())
            {
                return LoadLogoFromEpf();
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading logo image: {ex.Message}");
            // Handle image loading failure
        }

        return false;
    }

    private bool LoadLogoData()
    {
        var logoDataPath = _logoId > 0 ? $"logo{_logoId}.dat" : "logo.dat";
            
        if (!File.Exists(logoDataPath))
            return false;

        try
        {
            using var stream = File.OpenRead(logoDataPath);
            using var reader = new BinaryReader(stream);
            _logoData = reader.ReadBytes((int)stream.Length);
                    
            if (_logoData.Length > 0)
            {
                if (!DecompressLogoData())
                {
                    return false;
                }
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private bool LoadLogoFromEpf()
    {
        if (!File.Exists("logo.epf") || !File.Exists("logo.pal"))
            return false;

        try
        {
            using var epfStream = File.OpenRead("logo.epf");
            using var epfReader = new BinaryReader(epfStream);
            using var palStream = File.OpenRead("logo.pal");
            using var palReader = new BinaryReader(palStream);
            _logoData = epfReader.ReadBytes((int)epfStream.Length);
            _paletteData = palReader.ReadBytes(768);

            if (_logoData.Length > 0 && _paletteData.Length == 768)
            {
                if (!DecompressLogoData())
                {
                    return false;
                }
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private bool DecompressLogoData()
    {
        if (_logoData == null || _logoData.Length == 0)
            return false;

        try
        {
            if (_logoData.Length >= 8)
            {
                using var stream = new MemoryStream(_logoData);
                using var reader = new BinaryReader(stream);
                _logoWidth = reader.ReadInt16();
                _logoHeight = reader.ReadInt16();
                _logoX = reader.ReadInt16();
                _logoY = reader.ReadInt16();

                if (_logoWidth > 0 && _logoHeight > 0)
                {
                    var compressedData = reader.ReadBytes(_logoData.Length - 8);
                    var decompressedData = new byte[_logoWidth * _logoHeight];

                    if (DecompressImageData(compressedData, decompressedData))
                    {
                        var indexedImage = new IndexedImage(_logoWidth, _logoHeight, decompressedData);
                        var palette = CreatePalette();
                                
                        SetImage(indexedImage, palette);
                        _isLoaded = true;
                        return true;
                    }
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private bool DecompressImageData(byte[] compressedData, byte[] decompressedData)
    {
        if (compressedData == null || compressedData.Length == 0)
            return false;

        try
        {
            var srcIndex = 0;
            var dstIndex = 0;

            while (srcIndex < compressedData.Length && dstIndex < decompressedData.Length)
            {
                var control = compressedData[srcIndex++];

                if ((control & 0x80) != 0)
                {
                    var count = control & 0x7F;
                    if (count == 0)
                    {
                        count = compressedData[srcIndex++] + 128;
                    }

                    if (srcIndex < compressedData.Length)
                    {
                        var value = compressedData[srcIndex++];
                        for (var i = 0; i < count && dstIndex < decompressedData.Length; i++)
                        {
                            decompressedData[dstIndex++] = value;
                        }
                    }
                }
                else
                {
                    int count = control;
                    for (var i = 0; i < count && srcIndex < compressedData.Length && dstIndex < decompressedData.Length; i++)
                    {
                        decompressedData[dstIndex++] = compressedData[srcIndex++];
                    }
                }
            }

            return dstIndex == decompressedData.Length;
        }
        catch
        {
            return false;
        }
    }

    private Palette CreatePalette()
    {
        if (_paletteData != null && _paletteData.Length == 768)
        {
            var palette = new Palette(256);
                
            for (var i = 0; i < 256; i++)
            {
                var offset = i * 3;
                var r = _paletteData[offset];
                var g = _paletteData[offset + 1];
                var b = _paletteData[offset + 2];
                    
                palette.SetColor(i, Color.FromArgb(r, g, b));
            }
                
            return palette;
        }
        else
        {
            return PaletteManager.GetPalette("default");
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible || !_isLoaded)
            return;

        base.Render(spriteBatch);
    }

    public bool IsLogoLoaded => _isLoaded;
    public short LogoId => _logoId;
    public int LogoWidth => _logoWidth;
    public int LogoHeight => _logoHeight;
}
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace DarkAges.Library.Graphics;

/// <summary>
/// Handles ground/terrain image rendering and management for the world view
/// </summary>
[SuppressUnmanagedCodeSecurity]
public class WorldGroundImage : IDisposable
{
    private IntPtr _vftable;
    private byte[] _groundData;
    private ushort[] _tileData;
    private int _width;
    private int _height;
    private int _centerX;
    private int _centerY;
    private int _lastCenterX;
    private int _lastCenterY;
    private bool _isDirty;
    private bool _isInitialized;
    private readonly object _lockObject = new object();

    /// <summary>
    /// Gets the width of the ground image
    /// </summary>
    public int Width => _width;
        
    /// <summary>
    /// Gets the height of the ground image
    /// </summary>
    public int Height => _height;
        
    /// <summary>
    /// Gets or sets the center X coordinate
    /// </summary>
    public int CenterX
    {
        get => _centerX;
        set
        {
            if (_centerX != value)
            {
                _centerX = value;
                _isDirty = true;
            }
        }
    }
        
    /// <summary>
    /// Gets or sets the center Y coordinate
    /// </summary>
    public int CenterY
    {
        get => _centerY;
        set
        {
            if (_centerY != value)
            {
                _centerY = value;
                _isDirty = true;
            }
        }
    }
        
    /// <summary>
    /// Gets whether the ground image needs to be updated
    /// </summary>
    public bool IsDirty => _isDirty;

    /// <summary>
    /// Initializes a new instance of the WorldGroundImage class
    /// </summary>
    /// <param name="width">The width of the ground image</param>
    /// <param name="height">The height of the ground image</param>
    public WorldGroundImage(int width, int height)
    {
        _vftable = IntPtr.Zero;
        _width = 0;
        _height = 0;
        _centerX = 0;
        _centerY = 0;
        _lastCenterX = 0;
        _lastCenterY = 0;
        _isDirty = false;
        _isInitialized = false;
            
        Initialize(width, height);
    }
        
    /// <summary>
    /// Destructor
    /// </summary>
    ~WorldGroundImage()
    {
        Dispose(false);
    }

    /// <summary>
    /// Initializes the ground image with the specified dimensions
    /// </summary>
    /// <param name="width">The width of the ground image</param>
    /// <param name="height">The height of the ground image</param>
    public void Initialize(int width, int height)
    {
        lock (_lockObject)
        {
            _width = Math.Max(width, 1);
            _height = Math.Max(height, 1);
                
            // Calculate optimal size based on tile dimensions
            var optimalSize = Math.Max(_width / 56, _height / 28);
            optimalSize = 2 * optimalSize + 6;
                
            _groundData = new byte[optimalSize * optimalSize];
            _tileData = new ushort[optimalSize * optimalSize];
                
            Clear();
            _isInitialized = true;
        }
    }
        
    /// <summary>
    /// Updates the ground image based on current center position
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldY">World Y coordinate</param>
    /// <returns>True if the image was updated, false otherwise</returns>
    public bool Update(int worldX, int worldY)
    {
        lock (_lockObject)
        {
            if (!_isInitialized)
                return false;
                
            // Check if position has changed significantly
            if (!HasPositionChanged(worldX, worldY))
            {
                return false;
            }
                
            // Update center position
            _lastCenterX = _centerX;
            _lastCenterY = _centerY;
            _centerX = worldX;
            _centerY = worldY;
                
            // Update ground data
            UpdateGroundData();
                
            _isDirty = false;
            return true;
        }
    }
        
    /// <summary>
    /// Clears the ground image data
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            if (_groundData != null)
                Array.Clear(_groundData, 0, _groundData.Length);
                
            if (_tileData != null)
                Array.Clear(_tileData, 0, _tileData.Length);
                
            _isDirty = true;
        }
    }
        
    /// <summary>
    /// Sets the center position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    public void SetCenter(int x, int y)
    {
        CenterX = x;
        CenterY = y;
    }
        
    /// <summary>
    /// Gets the ground data at the specified position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>The ground data value</returns>
    public byte GetGroundData(int x, int y)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || x < 0 || x >= _width || y < 0 || y >= _height)
                return 0;
                
            var index = y * _width + x;
            return index < _groundData.Length ? _groundData[index] : (byte)0;
        }
    }
        
    /// <summary>
    /// Sets the ground data at the specified position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="value">The ground data value</param>
    public void SetGroundData(int x, int y, byte value)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || x < 0 || x >= _width || y < 0 || y >= _height)
                return;
                
            var index = y * _width + x;
            if (index < _groundData.Length)
            {
                _groundData[index] = value;
                _isDirty = true;
            }
        }
    }
        
    /// <summary>
    /// Gets the tile data at the specified position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>The tile data value</returns>
    public ushort GetTileData(int x, int y)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || x < 0 || x >= _width || y < 0 || y >= _height)
                return 0;
                
            var index = y * _width + x;
            return index < _tileData.Length ? _tileData[index] : (ushort)0;
        }
    }
        
    /// <summary>
    /// Sets the tile data at the specified position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="value">The tile data value</param>
    public void SetTileData(int x, int y, ushort value)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || x < 0 || x >= _width || y < 0 || y >= _height)
                return;
                
            var index = y * _width + x;
            if (index < _tileData.Length)
            {
                _tileData[index] = value;
                _isDirty = true;
            }
        }
    }
        
    /// <summary>
    /// Gets the ground data array
    /// </summary>
    /// <returns>A copy of the ground data array</returns>
    public byte[] GetGroundDataArray()
    {
        lock (_lockObject)
        {
            if (_groundData == null)
                return [];
                
            var copy = new byte[_groundData.Length];
            Array.Copy(_groundData, copy, _groundData.Length);
            return copy;
        }
    }
        
    /// <summary>
    /// Gets the tile data array
    /// </summary>
    /// <returns>A copy of the tile data array</returns>
    public ushort[] GetTileDataArray()
    {
        lock (_lockObject)
        {
            if (_tileData == null)
                return [];
                
            var copy = new ushort[_tileData.Length];
            Array.Copy(_tileData, copy, _tileData.Length);
            return copy;
        }
    }

    /// <summary>
    /// Checks if the position has changed significantly
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldY">World Y coordinate</param>
    /// <returns>True if position has changed, false otherwise</returns>
    private bool HasPositionChanged(int worldX, int worldY)
    {
        return _lastCenterX != worldX || _lastCenterY != worldY;
    }
        
    /// <summary>
    /// Updates the ground data based on current center position
    /// </summary>
    private void UpdateGroundData()
    {
        if (!_isInitialized)
            return;
            
        // Calculate visible area bounds
        var halfWidth = _width / 2;
        var halfHeight = _height / 2;
            
        var startX = _centerX - halfWidth;
        var startY = _centerY - halfHeight;
        var endX = _centerX + halfWidth;
        var endY = _centerY + halfHeight;
            
        // Update ground data for visible area
        for (var y = startY; y <= endY; y++)
        {
            for (var x = startX; x <= endX; x++)
            {
                var localX = x - startX;
                var localY = y - startY;
                    
                if (localX >= 0 && localX < _width && localY >= 0 && localY < _height)
                {
                    // Get ground tile from world
                    var tileId = GetWorldGroundTile(x, y);
                    SetTileData(localX, localY, tileId);
                        
                    // Set ground data based on tile type
                    var groundType = GetGroundTypeFromTile(tileId);
                    SetGroundData(localX, localY, groundType);
                }
            }
        }
    }
        
    /// <summary>
    /// Gets the ground tile from the world at the specified coordinates
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldY">World Y coordinate</param>
    /// <returns>The ground tile ID</returns>
    private ushort GetWorldGroundTile(int worldX, int worldY)
    {
        // This would typically query the world data
        // For now, return a simple pattern
        return (ushort)((worldX + worldY) % 256);
    }
        
    /// <summary>
    /// Gets the ground type from a tile ID
    /// </summary>
    /// <param name="tileId">The tile ID</param>
    /// <returns>The ground type</returns>
    private byte GetGroundTypeFromTile(ushort tileId)
    {
        // Simple mapping based on tile ID ranges
        if (tileId < 50)
            return 1; // Grass
        else if (tileId < 100)
            return 2; // Dirt
        else if (tileId < 150)
            return 3; // Stone
        else if (tileId < 200)
            return 4; // Water
        else
            return 0; // Unknown
    }

    /// <summary>
    /// Disposes of the WorldGroundImage
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
        
    /// <summary>
    /// Disposes of the WorldGroundImage
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from destructor</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_lockObject)
            {
                _groundData = null;
                _tileData = null;
                _isInitialized = false;
            }
        }
    }
}
using System;
using System.Runtime.InteropServices;
using System.Security;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

/// <summary>
/// Handles the minimap pane display and interaction
/// </summary>
[SuppressUnmanagedCodeSecurity]
public class WorldMinimapPane : IDisposable
{
    private IntPtr _vftable;
    private byte[] _minimapData;
    private byte[] _visibilityData;
    private int _width;
    private int _height;
    private int _zoomLevel;
    private int _centerX;
    private int _centerY;
    private int _offsetX;
    private int _offsetY;
    private bool _isVisible;
    private bool _isDirty;
    private bool _isInitialized;
    private readonly object _lockObject = new object();
        
    // Color palette for different terrain types
    private readonly uint[] _terrainColors = new uint[256];

    /// <summary>
    /// Gets the width of the minimap
    /// </summary>
    public int Width => _width;
        
    /// <summary>
    /// Gets the height of the minimap
    /// </summary>
    public int Height => _height;
        
    /// <summary>
    /// Gets or sets the zoom level
    /// </summary>
    public int ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            if (_zoomLevel != value)
            {
                _zoomLevel = Math.Max(1, Math.Min(10, value));
                _isDirty = true;
            }
        }
    }
        
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
    /// Gets or sets the X offset
    /// </summary>
    public int OffsetX
    {
        get => _offsetX;
        set
        {
            if (_offsetX != value)
            {
                _offsetX = value;
                _isDirty = true;
            }
        }
    }
        
    /// <summary>
    /// Gets or sets the Y offset
    /// </summary>
    public int OffsetY
    {
        get => _offsetY;
        set
        {
            if (_offsetY != value)
            {
                _offsetY = value;
                _isDirty = true;
            }
        }
    }
        
    /// <summary>
    /// Gets or sets whether the minimap is visible
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                _isDirty = true;
            }
        }
    }
        
    /// <summary>
    /// Gets whether the minimap needs to be updated
    /// </summary>
    public bool IsDirty => _isDirty;

    /// <summary>
    /// Initializes a new instance of the WorldMinimapPane class
    /// </summary>
    /// <param name="worldId">The world ID</param>
    public WorldMinimapPane(int worldId)
    {
        _vftable = IntPtr.Zero;
        _isVisible = true;
        _isDirty = false;
        _isInitialized = false;

        LoadLayout();
        Initialize(worldId);
    }
        
    /// <summary>
    /// Destructor
    /// </summary>
    ~WorldMinimapPane()
    {
        Dispose(false);
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_worldminimap.txt");
            _width = layout.GetInt("Width", 256);
            _height = layout.GetInt("Height", 256);
            _zoomLevel = layout.GetInt("ZoomLevel", 5);
            _centerX = layout.GetInt("CenterX", 0);
            _centerY = layout.GetInt("CenterY", 0);
            _offsetX = layout.GetInt("OffsetX", 0);
            _offsetY = layout.GetInt("OffsetY", 0);
                
            var colorsSection = layout.GetSection("TerrainColors");
            if (colorsSection != null)
            {
                foreach (KeyValuePair<string, string> entry in colorsSection)
                {
                    var index = int.Parse(entry.Key);
                    var color = uint.Parse(entry.Value, System.Globalization.NumberStyles.HexNumber);
                    if (index >= 0 && index < _terrainColors.Length)
                    {
                        _terrainColors[index] = color;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading world minimap layout: {ex.Message}");
            _width = 256;
            _height = 256;
            _zoomLevel = 5;
            _centerX = 0;
            _centerY = 0;
            _offsetX = 0;
            _offsetY = 0;
            InitializeTerrainColors();
        }
    }

    /// <summary>
    /// Initializes the minimap with the specified world ID
    /// </summary>
    /// <param name="worldId">The world ID</param>
    public void Initialize(int worldId)
    {
        lock (_lockObject)
        {
            _minimapData = new byte[_width * _height * 3]; // RGB data
            _visibilityData = new byte[_width * _height];
                
            InitializeTerrainColors();
            LoadWorldData(worldId);
                
            _isInitialized = true;
            _isDirty = true;
        }
    }
        
    /// <summary>
    /// Updates the minimap display
    /// </summary>
    /// <returns>True if the minimap was updated, false otherwise</returns>
    public bool Update()
    {
        lock (_lockObject)
        {
            if (!_isInitialized || !_isVisible)
                return false;
                
            if (!_isDirty)
                return false;
                
            UpdateMinimapData();
            _isDirty = false;
            return true;
        }
    }
        
    /// <summary>
    /// Renders the minimap to the specified graphics context
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    public void Render(IntPtr graphics, int x, int y)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || !_isVisible)
                return;
                
            RenderMinimap(graphics, x, y);
        }
    }
        
    /// <summary>
    /// Handles mouse input on the minimap
    /// </summary>
    /// <param name="x">Mouse X coordinate</param>
    /// <param name="y">Mouse Y coordinate</param>
    /// <param name="button">Mouse button</param>
    /// <returns>True if the input was handled, false otherwise</returns>
    public bool HandleMouseInput(int x, int y, int button)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || !_isVisible)
                return false;
                
            return ProcessMouseInput(x, y, button);
        }
    }
        
    /// <summary>
    /// Handles keyboard input for the minimap
    /// </summary>
    /// <param name="keyCode">The key code</param>
    /// <returns>True if the input was handled, false otherwise</returns>
    public bool HandleKeyboardInput(int keyCode)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || !_isVisible)
                return false;
                
            return ProcessKeyboardInput(keyCode);
        }
    }
        
    /// <summary>
    /// Sets the center position of the minimap
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    public void SetCenter(int x, int y)
    {
        CenterX = x;
        CenterY = y;
    }
        
    /// <summary>
    /// Zooms in the minimap
    /// </summary>
    public void ZoomIn()
    {
        ZoomLevel = Math.Min(10, _zoomLevel + 1);
    }
        
    /// <summary>
    /// Zooms out the minimap
    /// </summary>
    public void ZoomOut()
    {
        ZoomLevel = Math.Max(1, _zoomLevel - 1);
    }
        
    /// <summary>
    /// Gets the minimap data at the specified position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>The minimap data value</returns>
    public byte GetMinimapData(int x, int y)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || x < 0 || x >= _width || y < 0 || y >= _height)
                return 0;
                
            var index = (y * _width + x) * 3;
            return index < _minimapData.Length ? _minimapData[index] : (byte)0;
        }
    }
        
    /// <summary>
    /// Sets the minimap data at the specified position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="value">The minimap data value</param>
    public void SetMinimapData(int x, int y, byte value)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || x < 0 || x >= _width || y < 0 || y >= _height)
                return;
                
            var index = (y * _width + x) * 3;
            if (index < _minimapData.Length)
            {
                _minimapData[index] = value;
                _isDirty = true;
            }
        }
    }
        
    /// <summary>
    /// Gets the visibility data at the specified position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>The visibility data value</returns>
    public byte GetVisibilityData(int x, int y)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || x < 0 || x >= _width || y < 0 || y >= _height)
                return 0;
                
            var index = y * _width + x;
            return index < _visibilityData.Length ? _visibilityData[index] : (byte)0;
        }
    }
        
    /// <summary>
    /// Sets the visibility data at the specified position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="value">The visibility data value</param>
    public void SetVisibilityData(int x, int y, byte value)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || x < 0 || x >= _width || y < 0 || y >= _height)
                return;
                
            var index = y * _width + x;
            if (index < _visibilityData.Length)
            {
                _visibilityData[index] = value;
                _isDirty = true;
            }
        }
    }

    /// <summary>
    /// Initializes the terrain color palette
    /// </summary>
    private void InitializeTerrainColors()
    {
        // Initialize default terrain colors
        for (var i = 0; i < _terrainColors.Length; i++)
        {
            _terrainColors[i] = 0x000000; // Black by default
        }
            
        // Set specific terrain colors
        _terrainColors[0] = 0x000000;   // Unknown
        _terrainColors[1] = 0x00FF00;   // Grass
        _terrainColors[2] = 0x8B4513;   // Dirt
        _terrainColors[3] = 0x808080;   // Stone
        _terrainColors[4] = 0x0000FF;   // Water
        _terrainColors[180] = 0xFFFFFF; // Special tile
    }
        
    /// <summary>
    /// Loads world data for the minimap
    /// </summary>
    /// <param name="worldId">The world ID</param>
    private void LoadWorldData(int worldId)
    {
        // This would typically load world data from files
        // For now, generate some sample data
        var random = new Random(worldId);
            
        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var terrainType = (byte)(random.Next(0, 5));
                SetMinimapData(x, y, terrainType);
                SetVisibilityData(x, y, (byte)(random.Next(0, 2)));
            }
        }
    }
        
    /// <summary>
    /// Updates the minimap data based on current settings
    /// </summary>
    private void UpdateMinimapData()
    {
        if (!_isInitialized)
            return;
            
        // Calculate visible area based on center and zoom
        var visibleWidth = _width / _zoomLevel;
        var visibleHeight = _height / _zoomLevel;
            
        var startX = _centerX - visibleWidth / 2;
        var startY = _centerY - visibleHeight / 2;
        var endX = _centerX + visibleWidth / 2;
        var endY = _centerY + visibleHeight / 2;
            
        // Update minimap data for visible area
        for (var y = startY; y <= endY; y++)
        {
            for (var x = startX; x <= endX; x++)
            {
                var localX = x - startX;
                var localY = y - startY;
                    
                if (localX >= 0 && localX < _width && localY >= 0 && localY < _height)
                {
                    // Get terrain data from world
                    var terrainType = GetWorldTerrainData(x, y);
                    SetMinimapData(localX, localY, terrainType);
                        
                    // Update visibility
                    var visibility = GetWorldVisibilityData(x, y);
                    SetVisibilityData(localX, localY, visibility);
                }
            }
        }
    }
        
    /// <summary>
    /// Gets terrain data from the world at the specified coordinates
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldY">World Y coordinate</param>
    /// <returns>The terrain type</returns>
    private byte GetWorldTerrainData(int worldX, int worldY)
    {
        // This would typically query the world data
        // For now, return a simple pattern
        return (byte)((worldX + worldY) % 5);
    }
        
    /// <summary>
    /// Gets visibility data from the world at the specified coordinates
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldY">World Y coordinate</param>
    /// <returns>The visibility value</returns>
    private byte GetWorldVisibilityData(int worldX, int worldY)
    {
        // This would typically query the world data
        // For now, return a simple pattern
        return (byte)((worldX + worldY) % 2);
    }
        
    /// <summary>
    /// Renders the minimap to the graphics context
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    private void RenderMinimap(IntPtr graphics, int x, int y)
    {
        if (!_isInitialized || graphics == IntPtr.Zero)
            return;
            
        // This would typically use the graphics API to render the minimap
        // For now, just mark as rendered
        _isDirty = false;
    }
        
    /// <summary>
    /// Processes mouse input on the minimap
    /// </summary>
    /// <param name="x">Mouse X coordinate</param>
    /// <param name="y">Mouse Y coordinate</param>
    /// <param name="button">Mouse button</param>
    /// <returns>True if the input was handled, false otherwise</returns>
    private bool ProcessMouseInput(int x, int y, int button)
    {
        // Check if click is within minimap bounds
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return false;
            
        // Handle different mouse buttons
        switch (button)
        {
        case 1: // Left click - center minimap
            SetCenter(x, y);
            return true;
                    
        case 2: // Right click - context menu
            // Could show minimap options
            return true;
                    
        default:
            return false;
        }
    }
        
    /// <summary>
    /// Processes keyboard input for the minimap
    /// </summary>
    /// <param name="keyCode">The key code</param>
    /// <returns>True if the input was handled, false otherwise</returns>
    private bool ProcessKeyboardInput(int keyCode)
    {
        switch (keyCode)
        {
        case 147: // Zoom in
            ZoomIn();
            return true;
                    
        case 148: // Zoom out
            ZoomOut();
            return true;
                    
        default:
            return false;
        }
    }

    /// <summary>
    /// Disposes of the WorldMinimapPane
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
        
    /// <summary>
    /// Disposes of the WorldMinimapPane
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from destructor</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_lockObject)
            {
                _minimapData = null;
                _visibilityData = null;
                _isInitialized = false;
            }
        }
    }
}
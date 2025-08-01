using System;
using System.Runtime.InteropServices;
using System.Security;
using DarkAges.Library.World;

namespace DarkAges.Library.UI;

/// <summary>
/// Handles the dragged world item pane display and interaction
/// </summary>
[SuppressUnmanagedCodeSecurity]
public class DraggedWorldItemPane : IDisposable
{
    private IntPtr _vftable;
    private WorldObject_Item _draggedItem;
    private int _dragStartX;
    private int _dragStartY;
    private int _currentX;
    private int _currentY;
    private int _offsetX;
    private int _offsetY;
    private bool _isDragging;
    private bool _isVisible;
    private bool _isInitialized;
    private readonly object _lockObject = new object();

    /// <summary>
    /// Gets or sets the dragged item
    /// </summary>
    public WorldObject_Item DraggedItem
    {
        get => _draggedItem;
        set => _draggedItem = value;
    }
        
    /// <summary>
    /// Gets the drag start X coordinate
    /// </summary>
    public int DragStartX => _dragStartX;
        
    /// <summary>
    /// Gets the drag start Y coordinate
    /// </summary>
    public int DragStartY => _dragStartY;
        
    /// <summary>
    /// Gets or sets the current X coordinate
    /// </summary>
    public int CurrentX
    {
        get => _currentX;
        set => _currentX = value;
    }
        
    /// <summary>
    /// Gets or sets the current Y coordinate
    /// </summary>
    public int CurrentY
    {
        get => _currentY;
        set => _currentY = value;
    }
        
    /// <summary>
    /// Gets or sets the X offset
    /// </summary>
    public int OffsetX
    {
        get => _offsetX;
        set => _offsetX = value;
    }
        
    /// <summary>
    /// Gets or sets the Y offset
    /// </summary>
    public int OffsetY
    {
        get => _offsetY;
        set => _offsetY = value;
    }
        
    /// <summary>
    /// Gets or sets whether the pane is dragging
    /// </summary>
    public bool IsDragging
    {
        get => _isDragging;
        set => _isDragging = value;
    }
        
    /// <summary>
    /// Gets or sets whether the pane is visible
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    /// <summary>
    /// Initializes a new instance of the DraggedWorldItemPane class
    /// </summary>
    /// <param name="item">The item being dragged</param>
    /// <param name="startX">Start X coordinate</param>
    /// <param name="startY">Start Y coordinate</param>
    public DraggedWorldItemPane(WorldObject_Item item, int startX, int startY)
    {
        _vftable = IntPtr.Zero;
        _draggedItem = item;
        _dragStartX = startX;
        _dragStartY = startY;
        _currentX = startX;
        _currentY = startY;
        _offsetX = 0;
        _offsetY = 0;
        _isDragging = false;
        _isVisible = true;
        _isInitialized = false;
            
        Initialize();
    }
        
    /// <summary>
    /// Destructor
    /// </summary>
    ~DraggedWorldItemPane()
    {
        Dispose(false);
    }

    /// <summary>
    /// Initializes the dragged item pane
    /// </summary>
    public void Initialize()
    {
        lock (_lockObject)
        {
            if (_isInitialized)
                return;
                
            if (_draggedItem != null)
            {
                // Start dragging the item
                _draggedItem.StartDragging(_dragStartX, _dragStartY);
                _isDragging = true;
            }
                
            _isInitialized = true;
        }
    }
        
    /// <summary>
    /// Updates the dragged item pane
    /// </summary>
    /// <param name="deltaTime">Time since last update in milliseconds</param>
    public void Update(int deltaTime)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || !_isVisible)
                return;
                
            UpdateDragging();
        }
    }
        
    /// <summary>
    /// Renders the dragged item pane
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    public void Render(IntPtr graphics, int x, int y)
    {
        lock (_lockObject)
        {
            if (!_isInitialized || !_isVisible || !_isDragging)
                return;
                
            RenderDraggedItem(graphics, x, y);
        }
    }
        
    /// <summary>
    /// Updates the drag position
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    public void UpdateDragPosition(int x, int y)
    {
        lock (_lockObject)
        {
            if (!_isDragging)
                return;
                
            _currentX = x;
            _currentY = y;
                
            if (_draggedItem != null)
            {
                _draggedItem.UpdateDragPosition(x, y);
            }
        }
    }
        
    /// <summary>
    /// Stops dragging and drops the item
    /// </summary>
    /// <param name="dropX">Drop X coordinate</param>
    /// <param name="dropY">Drop Y coordinate</param>
    /// <returns>True if the item was dropped successfully, false otherwise</returns>
    public bool StopDragging(int dropX, int dropY)
    {
        lock (_lockObject)
        {
            if (!_isDragging)
                return false;
                
            _isDragging = false;
            _isVisible = false;
                
            var success = false;
            if (_draggedItem != null)
            {
                success = _draggedItem.StopDragging(dropX, dropY);
            }
                
            OnDragStopped(dropX, dropY, success);
            return success;
        }
    }
        
    /// <summary>
    /// Cancels the drag operation
    /// </summary>
    public void CancelDrag()
    {
        lock (_lockObject)
        {
            if (!_isDragging)
                return;
                
            _isDragging = false;
            _isVisible = false;
                
            if (_draggedItem != null)
            {
                // Return item to original position
                _draggedItem.StopDragging(_dragStartX, _dragStartY);
            }
                
            OnDragCancelled();
        }
    }
        
    /// <summary>
    /// Gets the drag information
    /// </summary>
    /// <param name="itemId">Output item ID</param>
    /// <param name="itemType">Output item type</param>
    /// <param name="quantity">Output quantity</param>
    /// <param name="quality">Output quality</param>
    /// <param name="startX">Output start X</param>
    /// <param name="startY">Output start Y</param>
    /// <param name="currentX">Output current X</param>
    /// <param name="currentY">Output current Y</param>
    /// <returns>True if drag information is available, false otherwise</returns>
    public bool GetDragInfo(out ushort itemId, out byte itemType, 
        out int quantity, out int quality,
        out int startX, out int startY,
        out int currentX, out int currentY)
    {
        lock (_lockObject)
        {
            if (_draggedItem != null)
            {
                _draggedItem.GetItemInfo(out itemId, out itemType, out quantity, out quality, out _);
            }
            else
            {
                itemId = 0;
                itemType = 0;
                quantity = 0;
                quality = 0;
            }
                
            startX = _dragStartX;
            startY = _dragStartY;
            currentX = _currentX;
            currentY = _currentY;
                
            return _isDragging;
        }
    }
        
    /// <summary>
    /// Handles mouse input on the dragged item pane
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
    /// Called when dragging is stopped
    /// </summary>
    /// <param name="dropX">Drop X coordinate</param>
    /// <param name="dropY">Drop Y coordinate</param>
    /// <param name="success">Whether the drop was successful</param>
    protected virtual void OnDragStopped(int dropX, int dropY, bool success)
    {
        // Override in derived classes
    }
        
    /// <summary>
    /// Called when dragging is cancelled
    /// </summary>
    protected virtual void OnDragCancelled()
    {
        // Override in derived classes
    }

    /// <summary>
    /// Updates the dragging state
    /// </summary>
    private void UpdateDragging()
    {
        if (_isDragging && _draggedItem != null)
        {
            // Update visual effects for dragging
            // This could include transparency, scaling, etc.
        }
    }
        
    /// <summary>
    /// Renders the dragged item
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    private void RenderDraggedItem(IntPtr graphics, int x, int y)
    {
        if (graphics == IntPtr.Zero || _draggedItem == null)
            return;
            
        // Calculate render position with offset
        var renderX = x + _offsetX;
        var renderY = y + _offsetY;
            
        // Render dragged item with special effects
        RenderDraggedItemEffect(graphics, renderX, renderY);
    }
        
    /// <summary>
    /// Renders the dragged item with special effects
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    private void RenderDraggedItemEffect(IntPtr graphics, int x, int y)
    {
        // Render item with dragging effects:
        // - Semi-transparent
        // - Slightly scaled up
        // - Drop shadow
        // - Highlight border
            
        if (_draggedItem == null)
            return;
            
        // Get item information
        _draggedItem.GetItemInfo(out var itemId, out var itemType, 
            out var quantity, out var quality, out var isStackable);
            
        // Render drop shadow
        RenderDropShadow(graphics, x + 2, y + 2);
            
        // Render item sprite with transparency
        RenderItemSprite(graphics, x, y, itemId, itemType);
            
        // Render quantity indicator if stackable
        if (isStackable && quantity > 1)
        {
            RenderQuantityIndicator(graphics, x, y, quantity);
        }
            
        // Render quality indicator
        if (quality > 0)
        {
            RenderQualityIndicator(graphics, x, y, quality);
        }
            
        // Render highlight border
        RenderHighlightBorder(graphics, x, y);
    }
        
    /// <summary>
    /// Renders the drop shadow
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    private void RenderDropShadow(IntPtr graphics, int x, int y)
    {
        // Render a semi-transparent black shadow
        // This would use the graphics API to draw the shadow
    }
        
    /// <summary>
    /// Renders the item sprite
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="itemType">Item type</param>
    private void RenderItemSprite(IntPtr graphics, int x, int y, ushort itemId, byte itemType)
    {
        // Render the item sprite with transparency
        // This would use the graphics API to draw the item sprite
    }
        
    /// <summary>
    /// Renders the quantity indicator
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="quantity">Quantity</param>
    private void RenderQuantityIndicator(IntPtr graphics, int x, int y, int quantity)
    {
        // Render quantity text in bottom-right corner
        // This would use the graphics API to draw the text
    }
        
    /// <summary>
    /// Renders the quality indicator
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="quality">Quality</param>
    private void RenderQualityIndicator(IntPtr graphics, int x, int y, int quality)
    {
        // Render quality indicator (color border, glow, etc.)
        // This would use the graphics API to draw the indicator
    }
        
    /// <summary>
    /// Renders the highlight border
    /// </summary>
    /// <param name="graphics">The graphics context</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    private void RenderHighlightBorder(IntPtr graphics, int x, int y)
    {
        // Render a highlight border around the item
        // This would use the graphics API to draw the border
    }
        
    /// <summary>
    /// Processes mouse input
    /// </summary>
    /// <param name="x">Mouse X coordinate</param>
    /// <param name="y">Mouse Y coordinate</param>
    /// <param name="button">Mouse button</param>
    /// <returns>True if the input was handled, false otherwise</returns>
    private bool ProcessMouseInput(int x, int y, int button)
    {
        // Check if click is within dragged item bounds
        if (x < _currentX - 16 || x > _currentX + 16 || 
            y < _currentY - 16 || y > _currentY + 16)
            return false;
            
        // Handle different mouse buttons
        switch (button)
        {
        case 1: // Left click - continue dragging
            UpdateDragPosition(x, y);
            return true;
                    
        case 2: // Right click - cancel drag
            CancelDrag();
            return true;
                    
        default:
            return false;
        }
    }

    /// <summary>
    /// Disposes of the DraggedWorldItemPane
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
        
    /// <summary>
    /// Disposes of the DraggedWorldItemPane
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from destructor</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_lockObject)
            {
                if (_isDragging)
                {
                    CancelDrag();
                }
                    
                _draggedItem = null;
                _isInitialized = false;
            }
        }
    }
}
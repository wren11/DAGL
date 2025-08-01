using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

/// <summary>
/// Background pane interface for GUI elements.
/// Provides a base background for UI components with event handling.
/// </summary>
public class GUIBackPane_Interface : ControlPane
{
    private bool isActive;
    private bool isDraggable;
    private bool isResizable;
    private int backgroundColor;
    private int borderColor;
    private int borderThickness;
    private List<ControlPane> childControls;
        
    /// <summary>
    /// Initializes a new instance of the GUIBackPane_Interface.
    /// </summary>
    public GUIBackPane_Interface()
    {
        InitializeVTable();
        InitializeDefaultProperties();
        LoadLayout();
    }
        
    /// <summary>
    /// Initializes the virtual function table pointers.
    /// </summary>
    private void InitializeVTable()
    {
        // Set vtable pointers (equivalent to C++ vftable assignment)
        // In C#, this is handled by the inheritance hierarchy
    }
        
    /// <summary>
    /// Initializes default properties for the background pane.
    /// </summary>
    private void InitializeDefaultProperties()
    {
        isActive = false;
        childControls = [];
    }
        
    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_guibackpane.txt");
            Bounds = layout.GetRect("Bounds", new Rectangle(0, 0, 200, 150));
            isDraggable = layout.GetBool("IsDraggable", false);
            isResizable = layout.GetBool("IsResizable", false);
            backgroundColor = layout.GetInt("BackgroundColor", 0x000000);
            borderColor = layout.GetInt("BorderColor", 0x808080);
            borderThickness = layout.GetInt("BorderThickness", 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading GUI back pane layout: {ex.Message}");
            // Fallback to defaults
            Bounds = new Rectangle(0, 0, 200, 150);
            isDraggable = false;
            isResizable = false;
            backgroundColor = 0x000000;
            borderColor = 0x808080;
            borderThickness = 1;
        }
    }
        
    /// <summary>
    /// Handles input events for the background pane.
    /// </summary>
    /// <param name="inputEvent">The input event to process</param>
    /// <returns>True if the event was handled, false otherwise</returns>
    public override bool HandleInput(Event inputEvent)
    {
        if (!IsVisible || !IsEnabled)
            return false;
                
        // Handle mouse events for dragging and resizing
        if (inputEvent is MouseEvent mouseEvent)
        {
            return HandleMouseEvent(mouseEvent);
        }
            
        // Handle key events
        if (inputEvent is KeyEvent keyEvent)
        {
            return HandleKeyEvent(keyEvent);
        }
            
        // Pass events to child controls
        return HandleChildEvents(inputEvent);
    }
        
    /// <summary>
    /// Handles mouse events for the background pane.
    /// </summary>
    /// <param name="mouseEvent">The mouse event to process</param>
    /// <returns>True if the event was handled, false otherwise</returns>
    private bool HandleMouseEvent(MouseEvent mouseEvent)
    {
        // Check if mouse is within the pane bounds
        if (!IsPointInBounds(mouseEvent.X, mouseEvent.Y))
            return false;
                
        switch (mouseEvent.Type)
        {
        case EventType.MouseDown:
            return HandleMouseDown(mouseEvent);
                    
        case EventType.MouseUp:
            return HandleMouseUp(mouseEvent);
                    
        case EventType.MouseMove:
            return HandleMouseMove(mouseEvent);
                    
        case EventType.MouseWheel:
            return HandleMouseWheel(mouseEvent);
        }
            
        return false;
    }
        
    /// <summary>
    /// Handles mouse down events.
    /// </summary>
    /// <param name="mouseEvent">The mouse event</param>
    /// <returns>True if handled</returns>
    private bool HandleMouseDown(MouseEvent mouseEvent)
    {
        isActive = true;
            
        // Handle dragging
        if (isDraggable && mouseEvent.Button == Core.Events.MouseButton.Left)
        {
            StartDragging(mouseEvent.X, mouseEvent.Y);
            return true;
        }
            
        // Handle resizing
        if (isResizable && mouseEvent.Button == Core.Events.MouseButton.Right)
        {
            StartResizing(mouseEvent.X, mouseEvent.Y);
            return true;
        }
            
        return false;
    }
        
    /// <summary>
    /// Handles mouse up events.
    /// </summary>
    /// <param name="mouseEvent">The mouse event</param>
    /// <returns>True if handled</returns>
    private bool HandleMouseUp(MouseEvent mouseEvent)
    {
        isActive = false;
            
        // Stop dragging or resizing
        StopDragging();
        StopResizing();
            
        return false;
    }
        
    /// <summary>
    /// Handles mouse move events.
    /// </summary>
    /// <param name="mouseEvent">The mouse event</param>
    /// <returns>True if handled</returns>
    private bool HandleMouseMove(MouseEvent mouseEvent)
    {
        if (!isActive)
            return false;
                
        // Update dragging position
        if (isDragging)
        {
            UpdateDragPosition(mouseEvent.X, mouseEvent.Y);
            return true;
        }
            
        // Update resizing
        if (isResizing)
        {
            UpdateResizePosition(mouseEvent.X, mouseEvent.Y);
            return true;
        }
            
        return false;
    }
        
    /// <summary>
    /// Handles mouse wheel events.
    /// </summary>
    /// <param name="mouseEvent">The mouse event</param>
    /// <returns>True if handled</returns>
    private bool HandleMouseWheel(MouseEvent mouseEvent)
    {
        // Handle scrolling or zooming
        return false;
    }
        
    /// <summary>
    /// Handles key events for the background pane.
    /// </summary>
    /// <param name="keyEvent">The key event to process</param>
    /// <returns>True if the event was handled, false otherwise</returns>
    private bool HandleKeyEvent(KeyEvent keyEvent)
    {
        if (!keyEvent.IsKeyDown)
            return false;
                
        switch (keyEvent.KeyCode)
        {
        case 27: // Escape key
            OnEscapePressed();
            return true;
                    
        case 13: // Enter key
            OnEnterPressed();
            return true;
                    
        case 9: // Tab key
            OnTabPressed();
            return true;
        }
            
        return false;
    }
        
    /// <summary>
    /// Handles input events for child controls.
    /// </summary>
    /// <param name="inputEvent">The input event</param>
    /// <returns>True if any child handled the event</returns>
    private bool HandleChildEvents(Event inputEvent)
    {
        // Process events for child controls in reverse order (top to bottom)
        for (var i = childControls.Count - 1; i >= 0; i--)
        {
            if (childControls[i].HandleInput(inputEvent))
            {
                return true;
            }
        }
            
        return false;
    }
        
    /// <summary>
    /// Adds a child control to this background pane.
    /// </summary>
    /// <param name="child">The child control to add</param>
    public new void AddChild(ControlPane child)
    {
        if (child != null && !childControls.Contains(child))
        {
            childControls.Add(child);
            child.SetParent(this);
        }
    }
        
    /// <summary>
    /// Removes a child control from this background pane.
    /// </summary>
    /// <param name="child">The child control to remove</param>
    public new void RemoveChild(ControlPane child)
    {
        if (child != null && childControls.Contains(child))
        {
            childControls.Remove(child);
            child.SetParent(null);
        }
    }
        
    /// <summary>
    /// Sets the background color of the pane.
    /// </summary>
    /// <param name="color">The background color (RGB format)</param>
    public void SetBackgroundColor(int color)
    {
        backgroundColor = color;
    }
        
    /// <summary>
    /// Sets the border color of the pane.
    /// </summary>
    /// <param name="color">The border color (RGB format)</param>
    public void SetBorderColor(int color)
    {
        borderColor = color;
    }
        
    /// <summary>
    /// Sets the border thickness of the pane.
    /// </summary>
    /// <param name="thickness">The border thickness in pixels</param>
    public void SetBorderThickness(int thickness)
    {
        borderThickness = Math.Max(0, thickness);
    }
        
    /// <summary>
    /// Enables or disables dragging for the pane.
    /// </summary>
    /// <param name="draggable">True to enable dragging, false to disable</param>
    public void SetDraggable(bool draggable)
    {
        isDraggable = draggable;
    }
        
    /// <summary>
    /// Enables or disables resizing for the pane.
    /// </summary>
    /// <param name="resizable">True to enable resizing, false to disable</param>
    public void SetResizable(bool resizable)
    {
        isResizable = resizable;
    }
        
    /// <summary>
    /// Renders the background pane and its children.
    /// </summary>
    /// <param name="spriteBatch">Graphics device for rendering</param>
    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible)
            return;
                
        // Render background
        RenderBackground(spriteBatch);
            
        // Render border
        if (borderThickness > 0)
        {
            RenderBorder(spriteBatch);
        }
            
        // Render child controls
        RenderChildren(spriteBatch);
    }
        
    /// <summary>
    /// Renders the background of the pane.
    /// </summary>
    /// <param name="spriteBatch">Graphics device for rendering</param>
    private void RenderBackground(SpriteBatch spriteBatch)
    {
        // Create a rectangle for the background
        var rect = new Rectangle(X, Y, Width, Height);
            
        // Fill the rectangle with the background color
        spriteBatch.FillRectangle(rect, Color.FromArgb(backgroundColor));
    }
        
    /// <summary>
    /// Renders the border of the pane.
    /// </summary>
    /// <param name="spriteBatch">Graphics device for rendering</param>
    private void RenderBorder(SpriteBatch spriteBatch)
    {
        // Draw border lines
        var leftLine = new Rectangle(X, Y, borderThickness, Height);
        var rightLine = new Rectangle(X + Width - borderThickness, Y, borderThickness, Height);
        var topLine = new Rectangle(X, Y, Width, borderThickness);
        var bottomLine = new Rectangle(X, Y + Height - borderThickness, Width, borderThickness);
            
        spriteBatch.FillRectangle(leftLine, Color.FromArgb(borderColor));
        spriteBatch.FillRectangle(rightLine, Color.FromArgb(borderColor));
        spriteBatch.FillRectangle(topLine, Color.FromArgb(borderColor));
        spriteBatch.FillRectangle(bottomLine, Color.FromArgb(borderColor));
    }
        
    /// <summary>
    /// Renders all child controls.
    /// </summary>
    /// <param name="spriteBatch">Graphics device for rendering</param>
    private void RenderChildren(SpriteBatch spriteBatch)
    {
        foreach (var child in childControls)
        {
            child.Render(spriteBatch);
        }
    }
        
    // Dragging and resizing support
    private bool isDragging;
    private bool isResizing;
    private int dragStartX, dragStartY;
    private int resizeStartX, resizeStartY;
    private int originalX, originalY;
    private int originalWidth, originalHeight;
        
    /// <summary>
    /// Starts dragging the pane.
    /// </summary>
    /// <param name="startX">Starting X coordinate</param>
    /// <param name="startY">Starting Y coordinate</param>
    private void StartDragging(int startX, int startY)
    {
        isDragging = true;
        dragStartX = startX;
        dragStartY = startY;
        originalX = X;
        originalY = Y;
    }
        
    /// <summary>
    /// Updates the drag position.
    /// </summary>
    /// <param name="currentX">Current X coordinate</param>
    /// <param name="currentY">Current Y coordinate</param>
    private void UpdateDragPosition(int currentX, int currentY)
    {
        var deltaX = currentX - dragStartX;
        var deltaY = currentY - dragStartY;
            
        X = originalX + deltaX;
        Y = originalY + deltaY;
    }
        
    /// <summary>
    /// Stops dragging the pane.
    /// </summary>
    private void StopDragging()
    {
        isDragging = false;
    }
        
    /// <summary>
    /// Starts resizing the pane.
    /// </summary>
    /// <param name="startX">Starting X coordinate</param>
    /// <param name="startY">Starting Y coordinate</param>
    private void StartResizing(int startX, int startY)
    {
        isResizing = true;
        resizeStartX = startX;
        resizeStartY = startY;
        originalWidth = Width;
        originalHeight = Height;
    }
        
    /// <summary>
    /// Updates the resize position.
    /// </summary>
    /// <param name="currentX">Current X coordinate</param>
    /// <param name="currentY">Current Y coordinate</param>
    private void UpdateResizePosition(int currentX, int currentY)
    {
        var deltaX = currentX - resizeStartX;
        var deltaY = currentY - resizeStartY;
            
        Width = Math.Max(50, originalWidth + deltaX);
        Height = Math.Max(50, originalHeight + deltaY);
    }
        
    /// <summary>
    /// Stops resizing the pane.
    /// </summary>
    private void StopResizing()
    {
        isResizing = false;
    }
        
    // Event handlers
    protected virtual void OnEscapePressed()
    {
        // Default behavior: hide the pane
        SetVisible(false);
    }
        
    protected virtual void OnEnterPressed()
    {
        // Default behavior: activate the pane
        isActive = true;
    }
        
    protected virtual void OnTabPressed()
    {
        // Default behavior: cycle focus between child controls
        CycleFocus();
    }
        
    /// <summary>
    /// Cycles focus between child controls.
    /// </summary>
    private void CycleFocus()
    {
        // Find the currently focused control
        var currentFocusIndex = -1;
        for (var i = 0; i < childControls.Count; i++)
        {
            if (childControls[i].HasFocus)
            {
                currentFocusIndex = i;
                break;
            }
        }
            
        // Clear current focus
        if (currentFocusIndex >= 0)
        {
            childControls[currentFocusIndex].SetFocus(false);
        }
            
        // Set focus to next control
        var nextFocusIndex = (currentFocusIndex + 1) % childControls.Count;
        if (childControls.Count > 0)
        {
            childControls[nextFocusIndex].SetFocus(true);
        }
    }
}
    
// These event types are now defined in DarkAges.Library.Core.Events
// Removing duplicates to avoid conflicts
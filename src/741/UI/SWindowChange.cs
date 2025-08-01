using System;
using System.Runtime.InteropServices;

namespace DarkAges.Library.UI;

/// <summary>
/// Window change event handler for managing window state changes.
/// Handles window resize, minimize, maximize, and focus events.
/// </summary>
public class SWindowChange : IDisposable
{
    private const int WINDOW_CHANGE_DATA_SIZE = 62;
        
    private bool isDisposed;
    private int windowState;
    private int previousWindowState;
    private int windowWidth;
    private int windowHeight;
    private int previousWidth;
    private int previousHeight;
    private bool hasFocus;
    private bool hadFocus;
    private DateTime lastChangeTime;
    private WindowChangeCallback? changeCallback;
        
    /// <summary>
    /// Delegate for window change callbacks.
    /// </summary>
    /// <param name="changeType">Type of window change</param>
    /// <param name="data">Additional change data</param>
    public delegate void WindowChangeCallback(WindowChangeType changeType, object data);

    public enum WindowChangeType
    {
        Resize,
        StateChange,
        FocusChange,
        Minimize,
        Maximize,
        Restore,
        Move,
        ZOrderChange,
        Generic
    }

    public struct WindowResizeData
    {
        public int NewWidth;
        public int NewHeight;
        public int OldWidth;
        public int OldHeight;
    }

    public struct WindowStateData
    {
        public WindowState NewState;
        public WindowState OldState;
    }

    public struct WindowFocusData
    {
        public bool HasFocus;
        public bool HadFocus;
    }

    public struct WindowMoveData
    {
        public int X;
        public int Y;
    }

    public struct WindowZOrderData
    {
        public int ZOrder;
    }

    public struct GenericWindowData
    {
        public int EventType;
        public IntPtr EventData;
        public int EventSize;
    }

    public enum WindowState
    {
        Normal,
        Minimized,
        Maximized
    }
        
    /// <summary>
    /// Initializes a new instance of the SWindowChange.
    /// </summary>
    public SWindowChange()
    {
        InitializeVTable();
        InitializeDefaultProperties();
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
    /// Initializes default properties for the window change handler.
    /// </summary>
    private void InitializeDefaultProperties()
    {
        isDisposed = false;
        windowState = (int)WindowState.Normal;
        previousWindowState = (int)WindowState.Normal;
        windowWidth = 800;
        windowHeight = 600;
        previousWidth = 800;
        previousHeight = 600;
        hasFocus = false;
        hadFocus = false;
        lastChangeTime = DateTime.Now;
        changeCallback = null;
    }
        
    /// <summary>
    /// Handles window change events.
    /// </summary>
    /// <param name="eventData">The event data containing window change information</param>
    /// <param name="eventType">Type of the event</param>
    /// <param name="eventSize">Size of the event data</param>
    /// <returns>True if the event was handled successfully, false otherwise</returns>
    public bool HandleWindowChange(IntPtr eventData, int eventType, int eventSize)
    {
        if (isDisposed || eventData == IntPtr.Zero)
            return false;
                
        try
        {
            // Process different types of window change events
            switch (eventType)
            {
            case 8: // Window resize event
                return HandleWindowResize(eventData, eventSize);
                        
            case 21: // Window state change event
                return HandleWindowStateChange(eventData, eventSize);
                        
            case 69: // Window focus event
                return HandleWindowFocus(eventData, eventSize);
                        
            case 102: // Window minimize/maximize event
                return HandleWindowMinMax(eventData, eventSize);
                        
            case 62: // Window change event
                return HandleGeneralWindowChange(eventData, eventSize);
                        
            default:
                return HandleUnknownEvent(eventData, eventType, eventSize);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling window change event: {ex.Message}");
            return false;
        }
    }
        
    /// <summary>
    /// Handles window resize events.
    /// </summary>
    /// <param name="eventData">Event data pointer</param>
    /// <param name="eventSize">Size of event data</param>
    /// <returns>True if handled successfully</returns>
    private bool HandleWindowResize(IntPtr eventData, int eventSize)
    {
        if (eventSize < 16) // Minimum size for resize event
            return false;
                
        // Read window dimensions from event data
        var newWidth = Marshal.ReadInt32(eventData, 0);
        var newHeight = Marshal.ReadInt32(eventData, 4);
            
        // Store previous dimensions
        previousWidth = windowWidth;
        previousHeight = windowHeight;
            
        // Update current dimensions
        windowWidth = newWidth;
        windowHeight = newHeight;
            
        // Check if dimensions actually changed
        if (windowWidth != previousWidth || windowHeight != previousHeight)
        {
            OnWindowResized(windowWidth, windowHeight, previousWidth, previousHeight);
            lastChangeTime = DateTime.Now;
        }
            
        return true;
    }
        
    /// <summary>
    /// Handles window state change events (minimize, maximize, restore).
    /// </summary>
    /// <param name="eventData">Event data pointer</param>
    /// <param name="eventSize">Size of event data</param>
    /// <returns>True if handled successfully</returns>
    private bool HandleWindowStateChange(IntPtr eventData, int eventSize)
    {
        if (eventSize < 4) // Minimum size for state change event
            return false;
                
        // Read new window state from event data
        var newState = Marshal.ReadInt32(eventData, 0);
            
        // Store previous state
        previousWindowState = windowState;
            
        // Update current state
        windowState = newState;
            
        // Check if state actually changed
        if (windowState != previousWindowState)
        {
            OnWindowStateChanged((WindowState)windowState, (WindowState)previousWindowState);
            lastChangeTime = DateTime.Now;
        }
            
        return true;
    }
        
    /// <summary>
    /// Handles window focus events.
    /// </summary>
    /// <param name="eventData">Event data pointer</param>
    /// <param name="eventSize">Size of event data</param>
    /// <returns>True if handled successfully</returns>
    private bool HandleWindowFocus(IntPtr eventData, int eventSize)
    {
        if (eventSize < 4) // Minimum size for focus event
            return false;
                
        // Read focus state from event data
        var focusState = Marshal.ReadInt32(eventData, 0);
            
        // Store previous focus state
        hadFocus = hasFocus;
            
        // Update current focus state
        hasFocus = focusState != 0;
            
        // Check if focus state changed
        if (hasFocus != hadFocus)
        {
            OnWindowFocusChanged(hasFocus, hadFocus);
            lastChangeTime = DateTime.Now;
        }
            
        return true;
    }
        
    /// <summary>
    /// Handles window minimize/maximize events.
    /// </summary>
    /// <param name="eventData">Event data pointer</param>
    /// <param name="eventSize">Size of event data</param>
    /// <returns>True if handled successfully</returns>
    private bool HandleWindowMinMax(IntPtr eventData, int eventSize)
    {
        if (eventSize < 4) // Minimum size for min/max event
            return false;
                
        // Read min/max state from event data
        var minMaxState = Marshal.ReadInt32(eventData, 0);
            
        // Handle minimize/maximize based on state
        switch (minMaxState)
        {
        case 1: // Minimize
            OnWindowMinimized();
            break;
                    
        case 2: // Maximize
            OnWindowMaximized();
            break;
                    
        case 3: // Restore
            OnWindowRestored();
            break;
        }
            
        lastChangeTime = DateTime.Now;
        return true;
    }
        
    /// <summary>
    /// Handles general window change events.
    /// </summary>
    /// <param name="eventData">Event data pointer</param>
    /// <param name="eventSize">Size of event data</param>
    /// <returns>True if handled successfully</returns>
    private bool HandleGeneralWindowChange(IntPtr eventData, int eventSize)
    {
        if (eventSize < 4) // Minimum size for general change event
            return false;
                
        // Read change type from event data
        var changeType = Marshal.ReadInt32(eventData, 0);
            
        // Handle different change types
        switch (changeType)
        {
        case 1: // Size change
            HandleSizeChange(eventData, eventSize);
            break;
                    
        case 2: // Position change
            HandlePositionChange(eventData, eventSize);
            break;
                    
        case 3: // Z-order change
            HandleZOrderChange(eventData, eventSize);
            break;
        }
            
        lastChangeTime = DateTime.Now;
        return true;
    }
        
    /// <summary>
    /// Handles unknown event types.
    /// </summary>
    /// <param name="eventData">Event data pointer</param>
    /// <param name="eventType">Event type</param>
    /// <param name="eventSize">Size of event data</param>
    /// <returns>True if handled successfully</returns>
    private bool HandleUnknownEvent(IntPtr eventData, int eventType, int eventSize)
    {
        // Log unknown event type for debugging
        Console.WriteLine($"Unknown window change event type: {eventType}, size: {eventSize}");
            
        // Try to handle as generic change event
        OnGenericWindowChange(eventType, eventData, eventSize);
            
        return true;
    }
        
    /// <summary>
    /// Handles size change events.
    /// </summary>
    /// <param name="eventData">Event data pointer</param>
    /// <param name="eventSize">Size of event data</param>
    private void HandleSizeChange(IntPtr eventData, int eventSize)
    {
        if (eventSize >= 8)
        {
            var newWidth = Marshal.ReadInt32(eventData, 4);
            var newHeight = Marshal.ReadInt32(eventData, 8);
                
            previousWidth = windowWidth;
            previousHeight = windowHeight;
            windowWidth = newWidth;
            windowHeight = newHeight;
                
            OnWindowResized(windowWidth, windowHeight, previousWidth, previousHeight);
        }
    }
        
    /// <summary>
    /// Handles position change events.
    /// </summary>
    /// <param name="eventData">Event data pointer</param>
    /// <param name="eventSize">Size of event data</param>
    private void HandlePositionChange(IntPtr eventData, int eventSize)
    {
        if (eventSize >= 8)
        {
            var newX = Marshal.ReadInt32(eventData, 4);
            var newY = Marshal.ReadInt32(eventData, 8);
                
            OnWindowMoved(newX, newY);
        }
    }
        
    /// <summary>
    /// Handles Z-order change events.
    /// </summary>
    /// <param name="eventData">Event data pointer</param>
    /// <param name="eventSize">Size of event data</param>
    private void HandleZOrderChange(IntPtr eventData, int eventSize)
    {
        if (eventSize >= 4)
        {
            var newZOrder = Marshal.ReadInt32(eventData, 4);
            OnWindowZOrderChanged(newZOrder);
        }
    }
        
    /// <summary>
    /// Sets the window change callback function.
    /// </summary>
    /// <param name="callback">The callback function to set</param>
    public void SetChangeCallback(WindowChangeCallback callback)
    {
        changeCallback = callback;
    }
        
    /// <summary>
    /// Gets the current window state.
    /// </summary>
    /// <returns>Current window state</returns>
    public WindowState GetWindowState()
    {
        return (WindowState)windowState;
    }
        
    /// <summary>
    /// Gets the current window dimensions.
    /// </summary>
    /// <param name="width">Output parameter for window width</param>
    /// <param name="height">Output parameter for window height</param>
    public void GetWindowDimensions(out int width, out int height)
    {
        width = windowWidth;
        height = windowHeight;
    }
        
    /// <summary>
    /// Gets whether the window currently has focus.
    /// </summary>
    /// <returns>True if window has focus, false otherwise</returns>
    public bool HasFocus()
    {
        return hasFocus;
    }
        
    /// <summary>
    /// Gets the timestamp of the last window change.
    /// </summary>
    /// <returns>Timestamp of last change</returns>
    public DateTime GetLastChangeTime()
    {
        return lastChangeTime;
    }
        
    // Event handlers
    protected virtual void OnWindowResized(int newWidth, int newHeight, int oldWidth, int oldHeight)
    {
        changeCallback?.Invoke(WindowChangeType.Resize, new WindowResizeData
        {
            NewWidth = newWidth,
            NewHeight = newHeight,
            OldWidth = oldWidth,
            OldHeight = oldHeight
        });
    }
        
    protected virtual void OnWindowStateChanged(WindowState newState, WindowState oldState)
    {
        changeCallback?.Invoke(WindowChangeType.StateChange, new WindowStateData
        {
            NewState = newState,
            OldState = oldState
        });
    }
        
    protected virtual void OnWindowFocusChanged(bool hasFocus, bool hadFocus)
    {
        changeCallback?.Invoke(WindowChangeType.FocusChange, new WindowFocusData
        {
            HasFocus = hasFocus,
            HadFocus = hadFocus
        });
    }
        
    protected virtual void OnWindowMinimized()
    {
        if (changeCallback != null)
            changeCallback(WindowChangeType.Minimize, null);
    }
        
    protected virtual void OnWindowMaximized()
    {
        if (changeCallback != null)
            changeCallback(WindowChangeType.Maximize, null);
    }
        
    protected virtual void OnWindowRestored()
    {
        if (changeCallback != null)
            changeCallback.Invoke(WindowChangeType.Restore, null);
    }
        
    protected virtual void OnWindowMoved(int newX, int newY)
    {
        changeCallback?.Invoke(WindowChangeType.Move, new WindowMoveData
        {
            X = newX,
            Y = newY
        });
    }
        
    protected virtual void OnWindowZOrderChanged(int newZOrder)
    {
        changeCallback?.Invoke(WindowChangeType.ZOrderChange, new WindowZOrderData
        {
            ZOrder = newZOrder
        });
    }
        
    protected virtual void OnGenericWindowChange(int eventType, IntPtr eventData, int eventSize)
    {
        changeCallback?.Invoke(WindowChangeType.Generic, new GenericWindowData
        {
            EventType = eventType,
            EventData = eventData,
            EventSize = eventSize
        });
    }
        
    /// <summary>
    /// Disposes of the window change handler.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
        
    /// <summary>
    /// Disposes of the window change handler.
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // Clean up managed resources
                changeCallback = null;
            }
                
            // Clean up unmanaged resources
            isDisposed = true;
        }
    }
        
    /// <summary>
    /// Finalizer for SWindowChange.
    /// </summary>
    ~SWindowChange()
    {
        Dispose(false);
    }
}
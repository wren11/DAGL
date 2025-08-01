using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;
using Size = System.Drawing.Size;

namespace DarkAges.Library.UI;

public class ScreenManager
{
    private static ScreenManager? _instance;
    public static ScreenManager Instance => _instance ??= new ScreenManager();

    private readonly List<Pane> _screens = new();
    private Pane? _currentScreen;
    private Pane? _modalScreen;
    private readonly List<ControlPane> _panes = new();
    private Size _screenSize;
    private bool _isDisposed;

    private ScreenManager()
    {
        LoadLayout();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_screen.txt");
            _screenSize = new Size(layout.GetInt("Width", 800), layout.GetInt("Height", 600));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading screen layout: {ex.Message}");
            _screenSize = new Size(800, 600);
        }
    }

    public void AddScreen(Pane screen)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ScreenManager));
            
        if (screen == null)
            throw new ArgumentNullException(nameof(screen));
            
        if (!_screens.Contains(screen))
        {
            _screens.Add(screen);
        }
    }

    public void RemoveScreen(Pane screen)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ScreenManager));
            
        if (screen == null)
            throw new ArgumentNullException(nameof(screen));
            
        _screens.Remove(screen);
        if (_currentScreen == screen)
            _currentScreen = null;
        if (_modalScreen == screen)
            _modalScreen = null;
    }

    public void SetCurrentScreen(Pane screen)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ScreenManager));
            
        if (screen == null)
            throw new ArgumentNullException(nameof(screen));
            
        if (_screens.Contains(screen))
        {
            _currentScreen = screen;
        }
    }

    public void ShowModal(Pane modalScreen)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ScreenManager));
            
        if (modalScreen == null)
            throw new ArgumentNullException(nameof(modalScreen));
            
        _modalScreen = modalScreen;
    }

    public void HideModal()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ScreenManager));
            
        _modalScreen = null;
    }

    public void AddPane(ControlPane pane)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ScreenManager));
            
        if (pane == null)
            throw new ArgumentNullException(nameof(pane));
            
        if (!_panes.Contains(pane))
            _panes.Add(pane);
    }

    public void AddPane(ControlPane pane, int zIndex)
    {
        AddPane(pane);
    }

    public void RemovePane(ControlPane pane)
    {
        _panes.Remove(pane);
    }

    public System.Drawing.Size GetScreenSize()
    {
        return _screenSize;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        // Render current screen
        _currentScreen?.Render(spriteBatch);

        // Render modal screen on top
        _modalScreen?.Render(spriteBatch);
    }

    public bool HandleEvent(Event e)
    {
        // Handle modal screen first
        if (_modalScreen != null && _modalScreen.HandleEvent(e))
            return true;

        // Handle current screen
        return _currentScreen?.HandleEvent(e) ?? false;
    }

    public void Update(float deltaTime)
    {
        _currentScreen?.Update(deltaTime);
        _modalScreen?.Update(deltaTime);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        foreach (var screen in _screens)
        {
            screen.Dispose();
        }
        _screens.Clear();
        
        foreach (var pane in _panes)
        {
            pane.Dispose();
        }
        _panes.Clear();
        
        _currentScreen = null;
        _modalScreen = null;
        _isDisposed = true;
    }
}
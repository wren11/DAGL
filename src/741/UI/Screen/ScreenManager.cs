using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Screen;

public class ScreenManager(GraphicsDevice graphicsDevice) : IDisposable
{
    private readonly List<ControlPane> _screens = [];
    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
    private readonly List<ControlPane> _panes = [];

    public void AddScreen(ControlPane screen)
    {
        _screens.Add(screen);
    }

    public void RemoveScreen(ControlPane screen)
    {
        _screens.Remove(screen);
    }

    public void SetCurrentScreen(ControlPane currentScreen)
    {
        foreach (var screen in _screens)
        {
            screen.IsVisible = screen == currentScreen;
        }
    }

    public void AddPane(ControlPane pane)
    {
        if (!_panes.Contains(pane))
            _panes.Add(pane);
    }

    public void AddPane(ControlPane pane, int zIndex)
    {
        // For now, ignore zIndex and just add the pane
        AddPane(pane);
    }

    public void RemovePane(ControlPane pane)
    {
        _panes.Remove(pane);
    }

    public System.Drawing.Size GetScreenSize()
    {
        // Return a default screen size; this can be made dynamic later
        return new System.Drawing.Size(800, 600);
    }

    public void Dispose()
    {
        foreach (var screen in _screens)
        {
            screen.Dispose();
        }
        _screens.Clear();
    }
}
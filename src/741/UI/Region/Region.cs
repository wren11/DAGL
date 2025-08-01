using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Region;

public class Region(int id, string name, string regionType = "default") : LObject(id, name)
{
    private readonly List<LObject> _objects = [];
    private readonly List<ControlPane> _uiElements = [];
    private ImagePane _backgroundImage;
    private Rectangle _bounds = new(0, 0, 800, 600);
    private bool _isActive;
    private string _regionType = regionType ?? throw new ArgumentNullException(nameof(regionType));

    public event EventHandler<RegionEventArgs> RegionEntered;
    public event EventHandler<RegionEventArgs> RegionExited;
    public event EventHandler<RegionEventArgs> RegionActivated;
    public event EventHandler<RegionEventArgs> RegionDeactivated;

    public void AddObject(LObject obj)
    {
        if (obj != null)
        {
            _objects.Add(obj);
        }
    }

    public void RemoveObject(LObject obj)
    {
        _objects.Remove(obj);
    }

    public void AddUIElement(ControlPane element)
    {
        if (element != null)
        {
            _uiElements.Add(element);
        }
    }

    public void RemoveUIElement(ControlPane element)
    {
        _uiElements.Remove(element);
    }

    public void SetBackgroundImage(string imagePath)
    {
        try
        {
            // Load the image using ImageLoader which returns IndexedImage
            var indexedImage = ImageLoader.LoadImage(imagePath);
            if (indexedImage != null)
            {
                _backgroundImage = new ImagePane();
                _backgroundImage.SetImage(indexedImage, null);
            }
            else
            {
                _backgroundImage = null;
            }
        }
        catch
        {
            _backgroundImage = null;
        }
    }

    public void SetBounds(Rectangle bounds)
    {
        _bounds = bounds;
    }

    public void Activate()
    {
        if (!_isActive)
        {
            _isActive = true;
            RegionActivated?.Invoke(this, new RegionEventArgs(this));
        }
    }

    public void Deactivate()
    {
        if (_isActive)
        {
            _isActive = false;
            RegionDeactivated?.Invoke(this, new RegionEventArgs(this));
        }
    }

    public void Enter()
    {
        RegionEntered?.Invoke(this, new RegionEventArgs(this));
    }

    public void Exit()
    {
        RegionExited?.Invoke(this, new RegionEventArgs(this));
    }

    public void Update(float deltaTime)
    {
        if (!_isActive) return;

        foreach (var obj in _objects)
        {
            obj.Update();
        }

        foreach (var element in _uiElements)
        {
            element.Update(deltaTime);
        }
    }

    public void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        if (_backgroundImage != null)
        {
            // spriteBatch.DrawImage(_backgroundImage, _bounds.X, _bounds.Y);
        }
        else
        {
            spriteBatch.DrawRectangle(_bounds, System.Drawing.Color.Transparent);
        }

        foreach (var obj in _objects)
        {
            obj.Render();
        }

        foreach (var element in _uiElements)
        {
            element.Render(spriteBatch);
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsEnabled || !_isActive) return false;

        if (e is MouseEvent mouseEvent)
        {
            if (!_bounds.Contains(mouseEvent.X, mouseEvent.Y))
                return false;
        }

        foreach (var element in _uiElements)
        {
            if (element.HandleEvent(e))
                return true;
        }

        foreach (var obj in _objects)
        {
            if (obj.HandleEvent(e))
                return true;
        }

        return false;
    }

    public override void Dispose()
    {
        foreach (var obj in _objects)
        {
            obj.Dispose();
        }
        _objects.Clear();

        foreach (var element in _uiElements)
        {
            element.Dispose();
        }
        _uiElements.Clear();

        _backgroundImage?.Dispose();
    }

    public T GetObject<T>(int id) where T : LObject
    {
        return _objects.Find(obj => obj.Id == id) as T;
    }

    public List<T> GetObjects<T>() where T : LObject
    {
        var result = new List<T>();
        foreach (var obj in _objects)
        {
            if (obj is T t)
            {
                result.Add(t);
            }
        }
        return result;
    }

    public bool ContainsPoint(Point point)
    {
        return _bounds.Contains(point);
    }

    public bool IntersectsWith(Rectangle rect)
    {
        return _bounds.IntersectsWith(rect);
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;

namespace DarkAges.Library.Graphics;

public class FrameAni : IDisposable
{
    private readonly List<FrameInfo> _frames = [];
    private int _currentFrameIndex;
    private float _frameTimer;
    private float _frameDuration = 0.1f;
    private bool _isPlaying;
    private bool _isLooping = true;
    private bool _isDisposed;

    public int CurrentFrameIndex => _currentFrameIndex;
    public int FrameCount => _frames.Count;
    public bool IsPlaying => _isPlaying;
    public bool IsLooping => _isLooping;
    public float FrameDuration => _frameDuration;

    public void AddFrame(FrameInfo frame)
    {
        if (_isDisposed) return;
        _frames.Add(frame);
    }

    public void AddFrame(IndexedImage image, Rectangle sourceRect, float duration = 0.1f)
    {
        if (_isDisposed) return;
        var frame = new FrameInfo(image, sourceRect, duration);
        _frames.Add(frame);
    }

    public void SetFrameDuration(float duration)
    {
        _frameDuration = duration;
    }

    public void Play()
    {
        if (_isDisposed || _frames.Count == 0) return;
        _isPlaying = true;
        _currentFrameIndex = 0;
        _frameTimer = 0;
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Stop()
    {
        _isPlaying = false;
        _currentFrameIndex = 0;
        _frameTimer = 0;
    }

    public void SetLooping(bool looping)
    {
        _isLooping = looping;
    }

    public void Update(float deltaTime)
    {
        if (_isDisposed || !_isPlaying || _frames.Count == 0) return;

        _frameTimer += deltaTime;
        var currentFrame = _frames[_currentFrameIndex];

        if (_frameTimer >= currentFrame.Duration)
        {
            _frameTimer = 0;
            _currentFrameIndex++;

            if (_currentFrameIndex >= _frames.Count)
            {
                if (_isLooping)
                {
                    _currentFrameIndex = 0;
                }
                else
                {
                    _isPlaying = false;
                    _currentFrameIndex = _frames.Count - 1;
                }
            }
        }
    }

    public void Render(float x, float y)
    {
        if (_isDisposed || _frames.Count == 0 || _currentFrameIndex >= _frames.Count) return;

        var currentFrame = _frames[_currentFrameIndex];
        var graphicsDevice = GraphicsDevice.Instance;

        graphicsDevice.DrawImage(currentFrame.Image, (int)x, (int)y, Color.White);
    }

    public void Render(float x, float y, float scale)
    {
        if (_isDisposed || _frames.Count == 0 || _currentFrameIndex >= _frames.Count) return;

        var currentFrame = _frames[_currentFrameIndex];
        var graphicsDevice = GraphicsDevice.Instance;

        var destRect = new Rectangle((int)x, (int)y, (int)(currentFrame.SourceRect.Width * scale), (int)(currentFrame.SourceRect.Height * scale));
        graphicsDevice.DrawImage(currentFrame.Image, destRect, currentFrame.SourceRect);
    }

    public void Render(float x, float y, Color tint)
    {
        if (_isDisposed || _frames.Count == 0 || _currentFrameIndex >= _frames.Count) return;

        var currentFrame = _frames[_currentFrameIndex];
        var graphicsDevice = GraphicsDevice.Instance;

        graphicsDevice.DrawImage(currentFrame.Image, (int)x, (int)y, tint);
    }

    public void Render(float x, float y, float scale, Color tint)
    {
        if (_isDisposed || _frames.Count == 0 || _currentFrameIndex >= _frames.Count) return;

        var currentFrame = _frames[_currentFrameIndex];
        var graphicsDevice = GraphicsDevice.Instance;

        var destRect = new Rectangle((int)x, (int)y, (int)(currentFrame.SourceRect.Width * scale), (int)(currentFrame.SourceRect.Height * scale));
        graphicsDevice.DrawImage(currentFrame.Image, destRect, currentFrame.SourceRect, tint);
    }

    public FrameInfo GetCurrentFrame()
    {
        if (_isDisposed || _frames.Count == 0 || _currentFrameIndex >= _frames.Count) return null;
        return _frames[_currentFrameIndex];
    }

    public void SetFrame(int frameIndex)
    {
        if (_isDisposed || frameIndex < 0 || frameIndex >= _frames.Count) return;
        _currentFrameIndex = frameIndex;
        _frameTimer = 0;
    }

    public void Reset()
    {
        _currentFrameIndex = 0;
        _frameTimer = 0;
    }

    public void Clear()
    {
        _frames.Clear();
        _currentFrameIndex = 0;
        _frameTimer = 0;
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        Clear();
        _isDisposed = true;
    }
}
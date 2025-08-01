using DarkAges.Library.Graphics;
using System.Diagnostics;

namespace DarkAges.Library.UI;

public class LogoShowPane : ControlPane
{
    private readonly ImagePane _logoImage;
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private readonly int _duration;

    public LogoShowPane(ImagePane logo, int duration)
    {
        _logoImage = logo;
        _duration = duration;
        AddChild(_logoImage);
        _stopwatch.Start();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;
            
        if (_stopwatch.ElapsedMilliseconds >= _duration)
        {
            IsVisible = false;
            // In a real implementation, this would fire an event
            // to signal the end of the logo sequence.
            return;
        }

        // The original implementation would have a more complex
        // palette animation. We'll simulate a simple fade-in.
        var progress = (float)_stopwatch.ElapsedMilliseconds / _duration;
            
        // This is a very simplified way to do a fade.
        // A real implementation would manipulate the palette directly.
        // For now, we'll just draw the image.
            
        base.Render(spriteBatch);
    }
}
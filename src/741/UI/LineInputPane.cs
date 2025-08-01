using DarkAges.Library.UI;
using System;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

public class LineInputPane : ControlPane
{
    private TextPane _label;
    private TextEditControlPane _textInput;

    public LineInputPane(short width, short height, int maxLength, int maxDisplayLength, string label)
    {
        _label = new TextPane(label, new Rectangle(), (FontManager.GetFont("default") as SimpleFont)!);
        _textInput = new TextEditControlPane();
        LoadLayout();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_lineinput.txt");
            _label.Bounds = layout.GetRect("Label", new Rectangle(0, 0, 100, 20));
            _textInput.Bounds = layout.GetRect("TextInput", new Rectangle(100, 0, 200, 20));
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error loading line input layout: {ex.Message}");
            _label.Bounds = new Rectangle(0, 0, 100, 20);
            _textInput.Bounds = new Rectangle(100, 0, 200, 20);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        base.Render(spriteBatch);
        _label.Render(spriteBatch);
        _textInput.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;
            
        return _textInput.HandleEvent(e);
    }

    public string GetText()
    {
        return _textInput.Text;
    }
}
using System.Collections.Generic;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

public class BtmButtonsPane_A : ControlPane
{
    private readonly List<ButtonControlPane> _buttons = [];

    public BtmButtonsPane_A()
    {
        var layout = new LayoutFileParser("lbtmbtn.txt");

        var buttonNames = layout.GetString("Buttons", "Button0,Button1,Button2").Split(',');
        foreach (var buttonName in buttonNames)
        {
            var button = new ButtonControlPane();
            var rect = layout.GetRect(buttonName);
            button.Bounds = rect;
            _buttons.Add(button);
            AddChild(button);
        }
    }
}
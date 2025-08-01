using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using Size = DarkAges.Library.Graphics.Size;

namespace DarkAges.Library.UI;

public class RadioGroupControlPane : ControlPane
{
    private readonly List<TextButtonExControlPane> _buttons = [];
    private int _selectedIndex = -1;
    private bool _checked = false;

    public event System.EventHandler<int> SelectionChanged;
    public event System.EventHandler CheckedChanged;

    public Point Position { get; set; }
    public Size Size { get; set; }

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked != value)
            {
                _checked = value;
                CheckedChanged?.Invoke(this, System.EventArgs.Empty);
            }
        }
    }

    public RadioGroupControlPane(params TextButtonExControlPane[] buttons)
    {
        foreach (var button in buttons)
        {
            _buttons.Add(button);
            AddChild(button);
            button.Click += OnButtonClicked;
        }
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value && value >= -1 && value < _buttons.Count)
            {
                _selectedIndex = value;
                UpdateButtonStates();
                SelectionChanged?.Invoke(this, _selectedIndex);
            }
        }
    }

    public string SelectedText
    {
        get
        {
            if (_selectedIndex >= 0 && _selectedIndex < _buttons.Count)
                return _buttons[_selectedIndex].Text;
            return string.Empty;
        }
    }

    private void OnButtonClicked(object sender, System.EventArgs e)
    {
        var clickedButton = sender as TextButtonExControlPane;
        if (clickedButton != null)
        {
            var index = _buttons.IndexOf(clickedButton);
            if (index >= 0)
            {
                SelectedIndex = index;
            }
        }
    }

    private void UpdateButtonStates()
    {
        for (var i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].IsPressed = (i == _selectedIndex);
        }
    }

    public void SetSelection(int index)
    {
        SelectedIndex = index;
    }

    public void ClearSelection()
    {
        SelectedIndex = -1;
    }

    public void AddButton(TextButtonExControlPane button)
    {
        _buttons.Add(button);
        AddChild(button);
        button.Click += OnButtonClicked;
    }

    public void RemoveButton(TextButtonExControlPane button)
    {
        var index = _buttons.IndexOf(button);
        if (index >= 0)
        {
            _buttons.RemoveAt(index);
            RemoveChild(button);
            button.Click -= OnButtonClicked;
                
            if (_selectedIndex == index)
            {
                _selectedIndex = -1;
            }
            else if (_selectedIndex > index)
            {
                _selectedIndex--;
            }
        }
    }
}
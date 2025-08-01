using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Dialogs
{
    public class NpcDialog : DialogPane
    {
        public event EventHandler<int> OptionSelected;

        private TextPane _textPane;
        private List<ButtonControlPane> _optionButtons;

        public NpcDialog(string text, List<string> options)
        {
            _textPane = new TextPane(text, new Rectangle(10, 10, 380, 100), FontManager.GetFont("default") as SimpleFont);
            AddChild(_textPane);

            _optionButtons = new List<ButtonControlPane>();
            for (int i = 0; i < options.Count; i++)
            {
                var button = new ButtonControlPane();
                button.SetText(options[i]);
                button.Bounds = new Rectangle(10, 120 + i * 30, 380, 25);
                int optionIndex = i;
                button.Click += (s, e) =>
                {
                    OptionSelected?.Invoke(this, optionIndex);
                    Hide();
                };
                _optionButtons.Add(button);
                AddChild(button);
            }
        }
    }
} 
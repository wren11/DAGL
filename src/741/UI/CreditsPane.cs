using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using DarkAges.Library.IO;
using Vector2 = DarkAges.Library.Graphics.Vector2;

namespace DarkAges.Library.UI;

public class CreditsPane : Pane
{
    private readonly List<CreditEntry> _credits = [];
    private TextButtonExControlPane _closeButton;
    private float _scrollOffset;
    private float _scrollSpeed;
    private DateTime _startTime;

    public CreditsPane()
    {
        _startTime = DateTime.Now;
        InitializeControls();
        LoadLayout();
        InitializeCredits();
    }

    private void InitializeControls()
    {
        _closeButton = new TextButtonExControlPane("Close");
        _closeButton.Click += OnCloseButtonClick;
        AddChild(_closeButton);
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_credits.txt");
            Bounds = layout.GetRect("Bounds", new Rectangle(100, 50, 600, 500));
            _scrollSpeed = layout.GetFloat("ScrollSpeed", 50f);
            _closeButton.Bounds = layout.GetRect("CloseButton", new Rectangle(250, 450, 100, 30));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading credits layout: {ex.Message}");
        }
    }

    private void InitializeCredits()
    {
        try
        {
            var layout = new LayoutFileParser("_credits.txt");
            var creditsSection = layout.GetSection("Credits");
            if (creditsSection != null)
            {
                foreach (var entry in creditsSection)
                {
                    var parts = entry.ToString().Split(';');
                    if (parts.Length < 2) continue;
                    CreditType type = (CreditType)Enum.Parse(typeof(CreditType), (ReadOnlySpan<char>)parts[0].Trim(), true);
                    var text = parts[1].Trim();
                    var color = Color.White;
                    if (parts.Length > 2)
                    {
                        color = ColorTranslator.FromHtml(parts[2].Trim());
                    }
                    _credits.Add(new CreditEntry(type, text, color));
                }
            }
            else
            {
                Console.WriteLine("No credits section found in layout.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading credits layout: {ex.Message}");
        }
    }

    private void OnCloseButtonClick(object sender, EventArgs e)
    {
        IsVisible = false;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        _scrollOffset -= _scrollSpeed * deltaTime;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);

        var font = FontManager.GetFont("default") as SimpleFont;
        if (font == null) return;

        var y = _scrollOffset;

        foreach (var credit in _credits)
        {
            var text = credit.Text;
            var color = credit.Color;
            var position = new Vector2(Bounds.Width / 2 - font.MeasureString(text).Width / 2, y);

            switch (credit.Type)
            {
            case CreditType.Title:
                spriteBatch.DrawString(font, text, position, color);
                y += 30;
                break;
            case CreditType.Category:
                spriteBatch.DrawString(font, text, position, color);
                y += 20;
                break;
            case CreditType.Item:
                spriteBatch.DrawString(font, text, position, color);
                y += 15;
                break;
            case CreditType.Spacer:
                y += 10;
                break;
            }
        }
    }
}
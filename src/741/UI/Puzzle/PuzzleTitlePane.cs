using System;
using System.Collections.Generic;
using DarkAges.Library.Graphics;
using System.Drawing;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleTitlePane : ControlPane
{
    public TextButtonExControlPane StartButton { get; private set; }
    public TextButtonExControlPane SettingButton { get; private set; }
    public TextButtonExControlPane RankingButton { get; private set; }
    public TextButtonExControlPane HelpButton { get; private set; }
    public TextButtonExControlPane ExitButton { get; private set; }

    public PuzzleTitlePane()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        StartButton = new TextButtonExControlPane("Start");
        SettingButton = new TextButtonExControlPane("Settings");
        RankingButton = new TextButtonExControlPane("Ranking");
        HelpButton = new TextButtonExControlPane("Help");
        ExitButton = new TextButtonExControlPane("Exit");

        StartButton.Position = new Point(200, 150);
        SettingButton.Position = new Point(200, 180);
        RankingButton.Position = new Point(200, 210);
        HelpButton.Position = new Point(200, 240);
        ExitButton.Position = new Point(200, 270);

        AddChild(StartButton);
        AddChild(SettingButton);
        AddChild(RankingButton);
        AddChild(HelpButton);
        AddChild(ExitButton);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Puzzle Game", 150, 50, Color.Black);
            
        base.Render(spriteBatch);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
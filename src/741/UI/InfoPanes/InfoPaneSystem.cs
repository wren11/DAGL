using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.InfoPanes;

public class InfoPaneSystem : ControlPane
{
    private readonly Dictionary<string, InfoPane> _infoPanes = new Dictionary<string, InfoPane>();
    private InfoPane _currentPane;

    public InfoPaneSystem()
    {
        InitializeInfoPanes();
    }

    private void InitializeInfoPanes()
    {
        _infoPanes["monster"] = new MonsterInfoPane();
        _infoPanes["monsterlist"] = new MonsterListPane();
        _infoPanes["town"] = new TownInfoPane();
        _infoPanes["worldmap"] = new NewWorldMapInfoPane();
        _infoPanes["help"] = new HelpInfoPane();
        _infoPanes["keyboard"] = new KeyboardInfoPane();
        _infoPanes["gui"] = new GUIInfoPane();
        _infoPanes["legend"] = new LegendInfoPane();
        _infoPanes["patch"] = new NewPatchPane();
    }

    public void ShowInfoPane(string paneType)
    {
        if (_infoPanes.ContainsKey(paneType))
        {
            _currentPane = _infoPanes[paneType];
            _currentPane.Show();
        }
    }

    public void HideCurrentPane()
    {
        _currentPane?.Hide();
        _currentPane = null;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _currentPane?.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        return _currentPane?.HandleEvent(e) ?? base.HandleEvent(e);
    }

    public override void Dispose()
    {
        foreach (var pane in _infoPanes.Values)
        {
            pane?.Dispose();
        }
        _infoPanes.Clear();
    }
}
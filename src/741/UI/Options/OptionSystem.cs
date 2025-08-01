using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Options;

public class OptionSystem : ControlPane
{
    private OptionPane _optionPane = null!;
    private GameSettingDialog _gameSettingDialog = null!;
    private Preference _preference = null!;

    public event EventHandler<Preference> PreferenceChanged = delegate { };

    public OptionSystem()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _optionPane = new OptionPane();
        _gameSettingDialog = new GameSettingDialog();
        _preference = new Preference();

        _optionPane.SettingChanged += (s, setting) => HandleSettingChange(setting);
        _gameSettingDialog.GameSettingChanged += (s, setting) => HandleGameSettingChange(setting);
    }

    private void HandleSettingChange(string setting)
    {
        _preference.SetValue(setting, true);
        PreferenceChanged?.Invoke(this, _preference);
    }

    private void HandleGameSettingChange(GameSetting setting)
    {
        _preference.SetGameSetting(setting);
        PreferenceChanged?.Invoke(this, _preference);
    }

    public void ShowOptions()
    {
        _optionPane.Show();
    }

    public void ShowGameSettings()
    {
        _gameSettingDialog.Show();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _optionPane.Render(spriteBatch);
        _gameSettingDialog.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_optionPane.HandleEvent(e)) return true;
        if (_gameSettingDialog.HandleEvent(e)) return true;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        _optionPane?.Dispose();
        _gameSettingDialog?.Dispose();
    }
}
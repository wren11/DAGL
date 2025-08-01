using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Options;

public class GameSettingDialog : ControlPane
{
    private readonly List<GameSetting> _settings = [];
    private readonly List<TextButtonExControlPane> _settingButtons = [];
    private TextButtonExControlPane _okButton = null!;
    private TextButtonExControlPane _cancelButton = null!;
    private TextButtonExControlPane _resetButton = null!;

    public event EventHandler<GameSetting> GameSettingChanged = delegate { };

    public GameSettingDialog()
    {
        InitializeSettings();
        InitializeControls();
    }

    private void InitializeSettings()
    {
        _settings.Add(new GameSetting { Name = "Sound Effects", Value = true, Category = "Audio" });
        _settings.Add(new GameSetting { Name = "Background Music", Value = true, Category = "Audio" });
        _settings.Add(new GameSetting { Name = "Show Chat", Value = true, Category = "Interface" });
        _settings.Add(new GameSetting { Name = "Show Names", Value = true, Category = "Interface" });
        _settings.Add(new GameSetting { Name = "Auto Attack", Value = false, Category = "Combat" });
        _settings.Add(new GameSetting { Name = "Auto Pickup", Value = false, Category = "Combat" });
    }

    private void InitializeControls()
    {
        for (var i = 0; i < _settings.Count; i++)
        {
            var setting = _settings[i];
            var button = new TextButtonExControlPane($"{setting.Name}: {(setting.Value ? "ON" : "OFF")}");
            button.Position = new Point(100, 100 + i * 30);
            button.Click += (s, e) => ToggleSetting(i);
            _settingButtons.Add(button);
            AddChild(button);
        }

        _okButton = new TextButtonExControlPane("OK");
        _cancelButton = new TextButtonExControlPane("Cancel");
        _resetButton = new TextButtonExControlPane("Reset");

        _okButton.Position = new Point(100, 350);
        _cancelButton.Position = new Point(200, 350);
        _resetButton.Position = new Point(300, 350);

        _okButton.Click += (s, e) => ApplySettings();
        _cancelButton.Click += (s, e) => Hide();
        _resetButton.Click += (s, e) => ResetSettings();

        AddChild(_okButton);
        AddChild(_cancelButton);
        AddChild(_resetButton);
    }

    private void ToggleSetting(int index)
    {
        if (index >= 0 && index < _settings.Count)
        {
            _settings[index].Value = !_settings[index].Value;
            _settingButtons[index].Text = $"{_settings[index].Name}: {(_settings[index].Value ? "ON" : "OFF")}";
        }
    }

    private void ApplySettings()
    {
        foreach (var setting in _settings)
        {
            GameSettingChanged?.Invoke(this, setting);
        }
        Hide();
    }

    private void ResetSettings()
    {
        for (var i = 0; i < _settings.Count; i++)
        {
            _settings[i].Value = _settings[i].DefaultValue;
            _settingButtons[i].Text = $"{_settings[i].Name}: {(_settings[i].Value ? "ON" : "OFF")}";
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var backgroundRect = new Rectangle(50, 50, 400, 400);
        spriteBatch.DrawRectangle(backgroundRect, System.Drawing.Color.White);
        spriteBatch.DrawRectangle(backgroundRect, System.Drawing.Color.Black);

        base.Render(spriteBatch);
    }
}
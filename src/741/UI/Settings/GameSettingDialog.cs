using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using Keys = Silk.NET.Input.Key;
using Size = DarkAges.Library.Graphics.Size;

namespace DarkAges.Library.UI.Settings;

public class GameSettingDialog : ControlPane
{
    private readonly List<GameSetting> _settings = [];
    private readonly List<ControlPane> _settingControls = [];
    private TextButtonExControlPane _saveButton;
    private TextButtonExControlPane _cancelButton;
    private TextButtonExControlPane _resetButton;
    private ImagePane _backgroundImage;
    private Rectangle _dialogBounds;
    private bool _isModal;
    private bool _hasChanges;

    public event EventHandler SettingsSaved;
    public event EventHandler SettingsCancelled;
    public event EventHandler<GameSettingEventArgs> SettingChanged;

    public GameSettingDialog()
    {
        InitializeDialog();
        LoadDefaultSettings();
    }

    private void InitializeDialog()
    {
        _saveButton = new TextButtonExControlPane("Save");
        _cancelButton = new TextButtonExControlPane("Cancel");
        _resetButton = new TextButtonExControlPane("Reset to Defaults");

        _saveButton.Position = new Point(200, 450);
        _cancelButton.Position = new Point(300, 450);
        _resetButton.Position = new Point(400, 450);

        _saveButton.Click += (s, e) => SaveSettings();
        _cancelButton.Click += (s, e) => CancelSettings();
        _resetButton.Click += (s, e) => ResetToDefaults();

        AddChild(_saveButton);
        AddChild(_cancelButton);
        AddChild(_resetButton);

        _dialogBounds = new Rectangle(50, 50, 600, 500);
        _isModal = true;
    }

    private void LoadDefaultSettings()
    {
        AddSetting(new GameSetting("Graphics", "Resolution", "800x600", SettingType.Dropdown, ["640x480", "800x600", "1024x768", "1280x1024"
        ]));
        AddSetting(new GameSetting("Graphics", "Fullscreen", "false", SettingType.Checkbox));
        AddSetting(new GameSetting("Graphics", "VSync", "true", SettingType.Checkbox));
        AddSetting(new GameSetting("Graphics", "AntiAliasing", "0", SettingType.Dropdown, ["0", "2x", "4x", "8x"]));

        AddSetting(new GameSetting("Audio", "Master Volume", "100", SettingType.Slider, minValue: 0, maxValue: 100));
        AddSetting(new GameSetting("Audio", "Music Volume", "80", SettingType.Slider, minValue: 0, maxValue: 100));
        AddSetting(new GameSetting("Audio", "Sound Volume", "90", SettingType.Slider, minValue: 0, maxValue: 100));
        AddSetting(new GameSetting("Audio", "Enable Music", "true", SettingType.Checkbox));
        AddSetting(new GameSetting("Audio", "Enable Sound Effects", "true", SettingType.Checkbox));

        AddSetting(new GameSetting("Gameplay", "Auto-Save Interval", "5", SettingType.Dropdown, ["1", "5", "10", "15", "30"
        ]));
        AddSetting(new GameSetting("Gameplay", "Show Damage Numbers", "true", SettingType.Checkbox));
        AddSetting(new GameSetting("Gameplay", "Show Experience Gain", "true", SettingType.Checkbox));
        AddSetting(new GameSetting("Gameplay", "Auto-Pickup Items", "false", SettingType.Checkbox));
        AddSetting(new GameSetting("Gameplay", "Confirm Item Deletion", "true", SettingType.Checkbox));

        AddSetting(new GameSetting("Interface", "Show Tooltips", "true", SettingType.Checkbox));
        AddSetting(new GameSetting("Interface", "Show Mini-Map", "true", SettingType.Checkbox));
        AddSetting(new GameSetting("Interface", "Show Chat Timestamps", "false", SettingType.Checkbox));
        AddSetting(new GameSetting("Interface", "Chat Window Opacity", "80", SettingType.Slider, minValue: 20, maxValue: 100));
        AddSetting(new GameSetting("Interface", "UI Scale", "100", SettingType.Slider, minValue: 50, maxValue: 200));

        AddSetting(new GameSetting("Network", "Connection Timeout", "30", SettingType.Slider, minValue: 10, maxValue: 60));
        AddSetting(new GameSetting("Network", "Auto-Reconnect", "true", SettingType.Checkbox));
        AddSetting(new GameSetting("Network", "Show Connection Status", "true", SettingType.Checkbox));

        AddSetting(new GameSetting("Performance", "Frame Rate Limit", "60", SettingType.Dropdown, ["30", "60", "120", "Unlimited"
        ]));
        AddSetting(new GameSetting("Performance", "Particle Effects", "true", SettingType.Checkbox));
        AddSetting(new GameSetting("Performance", "Shadow Quality", "Medium", SettingType.Dropdown, ["Low", "Medium", "High", "Ultra"
        ]));
        AddSetting(new GameSetting("Performance", "Texture Quality", "High", SettingType.Dropdown, ["Low", "Medium", "High", "Ultra"
        ]));
    }

    public void AddSetting(GameSetting setting)
    {
        _settings.Add(setting);
        CreateSettingControl(setting);
    }

    private void CreateSettingControl(GameSetting setting)
    {
        ControlPane control = null;
        var y = 80 + (_settingControls.Count * 35);

        switch (setting.Type)
        {
        case SettingType.Checkbox:
            var checkbox = new RadioGroupControlPane();
            checkbox.Position = new Point(400, y);
            checkbox.Size = new Size(20, 20);
            checkbox.Checked = bool.Parse(setting.Value);
            checkbox.CheckedChanged += (s, e) => OnSettingChanged(setting, checkbox.Checked.ToString());
            control = checkbox;
            break;

        case SettingType.Dropdown:
            var dropdown = new TextButtonExControlPane(setting.Value);
            dropdown.Position = new Point(400, y);
            dropdown.Size = new Size(150, 25);
            dropdown.Click += (s, e) => ShowDropdownOptions(setting, dropdown);
            control = dropdown;
            break;

        case SettingType.Slider:
            var slider = new TextEditControlPane();
            slider.Position = new Point(400, y);
            slider.Size = new Size(100, 25);
            slider.Text = setting.Value;
            slider.TextChanged += (s, e) => OnSettingChanged(setting, slider.Text);
            control = slider;
            break;

        case SettingType.Text:
            var textBox = new TextEditControlPane();
            textBox.Position = new Point(400, y);
            textBox.Size = new Size(200, 25);
            textBox.Text = setting.Value;
            textBox.TextChanged += (s, e) => OnSettingChanged(setting, textBox.Text);
            control = textBox;
            break;
        }

        if (control != null)
        {
            _settingControls.Add(control);
            AddChild(control);
        }
    }

    private void ShowDropdownOptions(GameSetting setting, TextButtonExControlPane dropdown)
    {
        if (setting.Options == null || setting.Options.Length == 0) return;

        // For now, just select the first option since NPCTextMenuDialog doesn't exist
        // In a real implementation, this would show a proper dropdown menu
        if (setting.Options.Length > 0)
        {
            setting.Value = setting.Options[0];
            dropdown.Text = setting.Options[0];
            OnSettingChanged(setting, setting.Options[0]);
        }
    }

    private void OnSettingChanged(GameSetting setting, string newValue)
    {
        setting.Value = newValue;
        _hasChanges = true;
        SettingChanged?.Invoke(this, new GameSettingEventArgs(setting));
    }

    private void SaveSettings()
    {
        try
        {
            foreach (var setting in _settings)
            {
                SaveSettingToRegistry(setting);
            }

            _hasChanges = false;
            SettingsSaved?.Invoke(this, EventArgs.Empty);
            Hide();
        }
        catch (Exception ex)
        {
            //var graphicsDevice = GraphicsDevice.Instance;
            //var font = FontManager.Instance.GetFont("default");
            //graphicsDevice.DrawText($"Failed to save settings: {ex.Message}", 200, 480, Color.Red, font);
        }
    }

    private void CancelSettings()
    {
        if (_hasChanges)
        {
            LoadSettingsFromRegistry();
        }
        SettingsCancelled?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    private void ResetToDefaults()
    {
        foreach (var setting in _settings)
        {
            setting.Value = setting.DefaultValue;
        }

        UpdateAllControls();
        _hasChanges = true;
    }

    private void UpdateAllControls()
    {
        for (var i = 0; i < _settings.Count && i < _settingControls.Count; i++)
        {
            var setting = _settings[i];
            var control = _settingControls[i];

            if (control is RadioGroupControlPane checkbox)
            {
                //checkbox.Checked = bool.Parse(setting.Value);
            }
            else if (control is TextButtonExControlPane dropdown)
            {
                dropdown.Text = setting.Value;
            }
            else if (control is TextEditControlPane textBox)
            {
                textBox.Text = setting.Value;
            }
        }
    }

    private void SaveSettingToRegistry(GameSetting setting)
    {
        var key = $"HKEY_CURRENT_USER\\Software\\DarkAges\\{setting.Category}\\{setting.Name}";
        Microsoft.Win32.Registry.SetValue(key, "Value", setting.Value);
    }

    private void LoadSettingsFromRegistry()
    {
        foreach (var setting in _settings)
        {
            try
            {
                var key = $"HKEY_CURRENT_USER\\Software\\DarkAges\\{setting.Category}\\{setting.Name}";
                var value = Microsoft.Win32.Registry.GetValue(key, "Value", setting.DefaultValue);
                setting.Value = value?.ToString() ?? setting.DefaultValue;
            }
            catch
            {
                setting.Value = setting.DefaultValue;
            }
        }

        UpdateAllControls();
        _hasChanges = false;
    }

    public GameSetting GetSetting(string category, string name)
    {
        return _settings.Find(s => s.Category == category && s.Name == name);
    }

    public void SetSetting(string category, string name, string value)
    {
        var setting = GetSetting(category, name);
        if (setting != null)
        {
            setting.Value = value;
            _hasChanges = true;
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        if (_backgroundImage != null)
        {
            //spriteBatch.DrawImage(_backgroundImage, _dialogBounds.X, _dialogBounds.Y);
        }
        else
        {
            spriteBatch.FillRectangle(_dialogBounds, Color.FromArgb(220, 0, 0, 0));
            spriteBatch.DrawRectangle(_dialogBounds, Color.White);
        }

        //spriteBatch.DrawString(font, "Game Settings", _dialogBounds.X + 250, _dialogBounds.Y + 20, Color.Yellow);

        var currentY = 80;
        var currentCategory = "";

        foreach (var setting in _settings)
        {
            if (setting.Category != currentCategory)
            {
                currentCategory = setting.Category;
                //spriteBatch.DrawString(font, currentCategory, _dialogBounds.X + 20, currentY, Color.Cyan);
                currentY += 25;
            }

            //spriteBatch.DrawString(font, setting.Name, _dialogBounds.X + 40, currentY, Color.White);
            currentY += 35;
        }

        if (_hasChanges)
        {
            //spriteBatch.DrawString(font, "* Unsaved Changes", _dialogBounds.X + 20, _dialogBounds.Y + 420, Color.Orange);
        }

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (e is KeyEvent keyEvent)
        {
            if (keyEvent.Type == EventType.KeyDown)
            {
                if (keyEvent.Key == Keys.Escape)
                {
                    CancelSettings();
                    return true;
                }
                else if (keyEvent.Key == Keys.Enter)
                {
                    SaveSettings();
                    return true;
                }
            }
        }

        return base.HandleEvent(e);
    }

    public void ShowDialog()
    {
        LoadSettingsFromRegistry();
        Show();
    }

    public void Hide()
    {
        IsVisible = false;
    }
}
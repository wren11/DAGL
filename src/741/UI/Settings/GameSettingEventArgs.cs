namespace DarkAges.Library.UI.Settings;

public class GameSettingEventArgs(GameSetting setting) : EventArgs
{
    public GameSetting Setting { get; } = setting ?? throw new ArgumentNullException(nameof(setting));
}
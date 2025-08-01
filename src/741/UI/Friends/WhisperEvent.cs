using DarkAges.Library.Core.Events;

namespace DarkAges.Library.UI.Friends;

public class WhisperEvent(string targetName, string message) : Event
{
    public string TargetName { get; set; } = targetName;
    public string Message { get; set; } = message;
}
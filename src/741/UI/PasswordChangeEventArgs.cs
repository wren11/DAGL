namespace DarkAges.Library.UI;

public class PasswordChangeEventArgs : EventArgs
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
    public bool Success { get; set; } = false;
    public string? ErrorMessage { get; set; }
}
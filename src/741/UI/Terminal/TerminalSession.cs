using System.Text;

namespace DarkAges.Library.UI.Terminal;

/// <summary>
/// Represents a terminal session
/// </summary>
public class TerminalSession(int id, TerminalConfiguration config)
{
    public int Id { get; } = id;
    public TerminalConfiguration Config { get; } = config;
    public bool IsConnected { get; private set; } = false;
    public DateTime Created { get; } = DateTime.Now;
    public DateTime LastActivity { get; private set; } = DateTime.Now;

    private StringBuilder terminalBuffer = new();
    private List<string> history = [];
    private int historyIndex = 0;

    public event Action<string> DataReceived;
    public event Action<string> DataSent;
    public event Action Closed;

    public void SetConnected(bool connected)
    {
        IsConnected = connected;
        LastActivity = DateTime.Now;
    }

    public void WriteToTerminal(string text)
    {
        terminalBuffer.Append(text);
        LastActivity = DateTime.Now;
        DataReceived?.Invoke(text);
    }

    public void SendData(string data)
    {
        LastActivity = DateTime.Now;
        DataSent?.Invoke(data);
    }

    public void AddToHistory(string command)
    {
        if (!string.IsNullOrEmpty(command))
        {
            history.Add(command);
            historyIndex = history.Count;
        }
    }

    public string GetHistoryEntry(int index)
    {
        if (index >= 0 && index < history.Count)
        {
            return history[index];
        }
        return null;
    }

    public string GetPreviousHistory()
    {
        if (historyIndex > 0)
        {
            historyIndex--;
            return GetHistoryEntry(historyIndex);
        }
        return null;
    }

    public string GetNextHistory()
    {
        if (historyIndex < history.Count - 1)
        {
            historyIndex++;
            return GetHistoryEntry(historyIndex);
        }
        return null;
    }

    public void Close()
    {
        IsConnected = false;
        Closed?.Invoke();
    }

    public string GetTerminalContent()
    {
        return terminalBuffer.ToString();
    }

    public void ClearTerminal()
    {
        terminalBuffer.Clear();
    }
}
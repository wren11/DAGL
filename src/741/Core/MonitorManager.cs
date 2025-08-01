namespace DarkAges.Library.Core;

public class MonitorManager
{
    private readonly List<Monitor> _monitors = [];
    private bool _isEnabled = true;

    public bool IsEnabled => _isEnabled;
    public int MonitorCount => _monitors.Count;

    public void AddMonitor(Monitor monitor)
    {
        _monitors.Add(monitor);
    }

    public void RemoveMonitor(Monitor monitor)
    {
        _monitors.Remove(monitor);
    }

    public void Enable()
    {
        _isEnabled = true;
    }

    public void Disable()
    {
        _isEnabled = false;
    }

    public void Update()
    {
        if (!_isEnabled) return;

        foreach (var monitor in _monitors)
        {
            monitor.Update();
        }
    }

    public void Clear()
    {
        foreach (var monitor in _monitors)
        {
            monitor?.Dispose();
        }
        _monitors.Clear();
    }

    public void Dispose()
    {
        Clear();
    }
}
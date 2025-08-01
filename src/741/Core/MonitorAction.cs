namespace DarkAges.Library.Core;

public abstract class MonitorAction : IDisposable
{
    protected bool _isDisposed;

    public abstract void Execute();

    public virtual void Dispose()
    {
        _isDisposed = true;
    }
}
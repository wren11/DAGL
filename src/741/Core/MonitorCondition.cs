namespace DarkAges.Library.Core;

public abstract class MonitorCondition : IDisposable
{
    protected bool _isDisposed;

    public abstract bool Evaluate();

    public virtual void Dispose()
    {
        _isDisposed = true;
    }
}
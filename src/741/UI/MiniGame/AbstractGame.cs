namespace DarkAges.Library.UI.MiniGame;

public abstract class AbstractGame : ControlPane
{
    protected bool _isInitialized;
    protected bool _isRunning;
    protected int _score;
    protected int _level;

    public bool IsRunning => _isRunning;
    public int Score => _score;
    public int Level => _level;

    public abstract void Initialize();
    public abstract void Start();
    public abstract void Pause();
    public abstract void Resume();
    public abstract void End();
    public abstract void Reset();
}
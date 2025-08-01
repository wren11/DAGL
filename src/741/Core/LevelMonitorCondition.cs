namespace DarkAges.Library.Core;

public class LevelMonitorCondition(Func<int> getCurrentLevel, int targetLevel) : MonitorCondition
{
    public override bool Evaluate()
    {
        if (_isDisposed) return false;

        var currentLevel = getCurrentLevel();
        return currentLevel >= targetLevel;
    }
}
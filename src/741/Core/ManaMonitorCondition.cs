namespace DarkAges.Library.Core;

public class ManaMonitorCondition(Func<int> getCurrentMana, Func<int> getMaxMana, float threshold = 0.3f)
        : MonitorCondition
{
    public override bool Evaluate()
    {
        if (_isDisposed) return false;

        var currentMana = getCurrentMana();
        var maxMana = getMaxMana();

        if (maxMana <= 0) return false;

        var manaPercentage = (float)currentMana / maxMana;
        return manaPercentage <= threshold;
    }
}
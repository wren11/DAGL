namespace DarkAges.Library.Core;

public class HealthMonitorCondition(Func<int> getCurrentHealth, Func<int> getMaxHealth, float threshold = 0.5f)
        : MonitorCondition
{
    public override bool Evaluate()
    {
        if (_isDisposed) return false;

        var currentHealth = getCurrentHealth();
        var maxHealth = getMaxHealth();

        if (maxHealth <= 0) return false;

        var healthPercentage = (float)currentHealth / maxHealth;
        return healthPercentage <= threshold;
    }
}
namespace DarkAges.Library.GameLogic;

/// <summary>
/// Helper extensions for status effects
/// </summary>
public static class StatusEffectExtensions
{
    public static bool Any(this IEnumerable<StatusEffect> effects, Func<StatusEffect, bool> predicate)
    {
        foreach (var effect in effects)
        {
            if (predicate(effect))
                return true;
        }
        return false;
    }
}
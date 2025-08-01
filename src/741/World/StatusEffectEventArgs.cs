using DarkAges.Library.GameLogic;

namespace DarkAges.Library.World;

public class StatusEffectEventArgs(StatusEffect effect) : EventArgs
{
    public StatusEffect Effect { get; } = effect;
}
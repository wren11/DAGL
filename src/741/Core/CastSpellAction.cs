namespace DarkAges.Library.Core;

public class CastSpellAction(Action<int> castSpell, int spellId) : MonitorAction
{
    public override void Execute()
    {
        if (_isDisposed) return;
        castSpell?.Invoke(spellId);
    }
}
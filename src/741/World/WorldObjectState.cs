namespace DarkAges.Library.World;

/// <summary>
/// States that world objects can be in
/// </summary>
public enum WorldObjectState
{
    Idle = 0,
    Moving = 1,
    Attacking = 2,
    Casting = 3,
    Dead = 4,
    Hidden = 5,
    Frozen = 6,
    Stunned = 7
}
using DarkAges.Library.Graphics;

namespace DarkAges.Library.World;

public class FindNearestPlayerEventArgs : EventArgs
{
    public Vector2 NpcPosition { get; set; }
    public float AggroRange { get; set; }
    public WorldObject_NPC RequestingNpc { get; set; } = null!;
    public WorldObject_Human? NearestPlayer { get; set; }
}
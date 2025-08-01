namespace DarkAges.Library.Graphics;

/// <summary>
/// World background image properties
/// </summary>
public struct WorldBackImageProperties
{
    public float ScaleX;
    public float ScaleY;
    public float OffsetX;
    public float OffsetY;
    public float Rotation;
    public ColorRgb555 BackgroundColor;
    public bool UseTransparency;
    public int TransparencyLevel;
    public bool IsAnimated;
    public float AnimationSpeed;
    public float AnimationOffset;
    public bool IsLooping;
    public int LayerCount;
}
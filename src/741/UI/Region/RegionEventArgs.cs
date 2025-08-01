namespace DarkAges.Library.UI.Region;

public class RegionEventArgs(Region region) : EventArgs
{
    public Region Region { get; } = region ?? throw new ArgumentNullException(nameof(region));
}
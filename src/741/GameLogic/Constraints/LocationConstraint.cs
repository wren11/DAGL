namespace DarkAges.Library.GameLogic.Constraints;

public class LocationConstraint(string location) : EventConstraint($"Location_{location}")
{
    public string RequiredLocation { get; set; } = location ?? throw new ArgumentNullException(nameof(location));
    public string CurrentLocation { get; set; }

    public override bool Evaluate()
    {
        return string.Equals(CurrentLocation, RequiredLocation, StringComparison.OrdinalIgnoreCase);
    }

    public void UpdateLocation(string location)
    {
        CurrentLocation = location;
    }
}
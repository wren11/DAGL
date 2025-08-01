namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Information about an event
/// </summary>
public class EventInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    public int IconId { get; set; }
    public string Requirements { get; set; }
    public List<EventReward> Rewards { get; set; } = [];
    public EventStatus Status { get; set; } = EventStatus.Unknown;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsRepeatable { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
}
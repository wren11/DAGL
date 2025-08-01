namespace DarkAges.Library.GameLogic.Constraints;

public class TimeConstraint(TimeSpan start, TimeSpan end) : EventConstraint($"Time_{start}_{end}")
{
    public TimeSpan StartTime { get; set; } = start;
    public TimeSpan EndTime { get; set; } = end;
    public bool IsActive { get; set; }

    public override bool Evaluate()
    {
        var now = DateTime.Now.TimeOfDay;
            
        if (StartTime <= EndTime)
        {
            return now >= StartTime && now <= EndTime;
        }
        else
        {
            return now >= StartTime || now <= EndTime;
        }
    }
}
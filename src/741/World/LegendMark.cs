namespace DarkAges.Library.World;

public class LegendMark
{
    public string Text { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public string Color { get; set; } = "White";
    public string Icon { get; set; } = "";
    public int Priority { get; set; } = 0;
}
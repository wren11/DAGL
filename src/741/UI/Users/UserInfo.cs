namespace DarkAges.Library.UI.Users;

public class UserInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public string Class { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastLogin { get; set; }
    public string CurrentLocation { get; set; }
    public int Experience { get; set; }
    public int Health { get; set; }
    public int Mana { get; set; }
}
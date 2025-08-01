namespace DarkAges.Library.UI.Friends;

public class FriendEntry
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsOnline { get; set; }
    public int Level { get; set; }
    public string Class { get; set; }
    public DateTime LastSeen { get; set; }
    public string CurrentLocation { get; set; }
}
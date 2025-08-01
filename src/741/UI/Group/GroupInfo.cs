namespace DarkAges.Library.UI.Group;

public class GroupInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int MemberCount { get; set; }
    public int MaxMembers { get; set; }
    public string LeaderName { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedDate { get; set; }
}
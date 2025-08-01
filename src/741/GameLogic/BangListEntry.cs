using System;

namespace DarkAges.Library.GameLogic;

public class BangListEntry(string name, string reason, string gmName)
{
    public string Name { get; set; } = name;
    public string Reason { get; set; } = reason;
    public string GmName { get; set; } = gmName;
}
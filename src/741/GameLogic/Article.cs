using System;

namespace DarkAges.Library.GameLogic;

public class Article
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int Number { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string? Author { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int TitleLength { get; set; }
    public byte AuthorLength { get; set; }
    public int Flags { get; set; }
    public int ContentLength { get; set; }
}
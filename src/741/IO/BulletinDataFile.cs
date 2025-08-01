using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DarkAges.Library.IO;

public class BulletinDataFile
{
    public List<GameLogic.Article> Articles { get; } = [];
        
    public BulletinDataFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        try
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);
            ParseBulletinData(reader);
        }
        catch
        {
            // Fallback to text format if binary parsing fails
            ParseTextFormat(filePath);
        }
    }

    private void ParseBulletinData(BinaryReader reader)
    {
        // Read file header
        var magic = reader.ReadInt32();
        if (magic != 0x4C42554C) // "LBLU" magic number
        {
            throw new InvalidDataException("Invalid bulletin data file format");
        }

        var version = reader.ReadInt32();
        var articleCount = reader.ReadInt32();
        var dataOffset = reader.ReadInt32();

        // Skip to article table
        reader.BaseStream.Position = dataOffset;

        // Read article headers
        for (var i = 0; i < articleCount; i++)
        {
            var article = new GameLogic.Article
            {
                Id = reader.ReadInt32(),
                ParentId = reader.ReadInt32(),
                Date = DateTime.FromBinary(reader.ReadInt64()),
                AuthorLength = reader.ReadByte(),
                TitleLength = reader.ReadInt16(),
                ContentLength = reader.ReadInt32(),
                Flags = reader.ReadInt32()
            };

            // Read author name
            var authorBytes = reader.ReadBytes(article.AuthorLength);
            article.Author = Encoding.ASCII.GetString(authorBytes);

            // Read title
            var titleBytes = reader.ReadBytes(article.TitleLength);
            article.Title = Encoding.ASCII.GetString(titleBytes);

            // Read content
            var contentBytes = reader.ReadBytes(article.ContentLength);
            article.Content = Encoding.ASCII.GetString(contentBytes);

            Articles.Add(article);
        }
    }

    private void ParseTextFormat(string filePath)
    {
        using var reader = new StreamReader(filePath);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrEmpty(line)) continue;

            var parts = line.Split('|');
            if (parts.Length >= 5)
            {
                var article = new GameLogic.Article
                {
                    Id = int.Parse(parts[0]),
                    ParentId = int.Parse(parts[1]),
                    Date = DateTime.Parse(parts[2]),
                    Author = parts[3],
                    Title = parts[4],
                    Content = parts.Length > 5 ? parts[5] : ""
                };
                Articles.Add(article);
            }
        }
    }

    public List<GameLogic.Article> GetArticles()
    {
        return [..Articles];
    }

    public GameLogic.Article GetArticle(int id)
    {
        return Articles.Find(a => a.Id == id);
    }

    public List<GameLogic.Article> GetReplies(int parentId)
    {
        return Articles.FindAll(a => a.ParentId == parentId);
    }

    public void SaveToFile(string filePath)
    {
        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream);
        // Write header
        writer.Write(0x4C42554C); // "LBLU" magic
        writer.Write(1); // Version
        writer.Write(Articles.Count);
        writer.Write(16); // Data offset

        // Write article data
        foreach (var article in Articles)
        {
            writer.Write(article.Id);
            writer.Write(article.ParentId);
            writer.Write(article.Date.ToBinary());
                    
            var authorBytes = Encoding.ASCII.GetBytes(article.Author);
            writer.Write((byte)authorBytes.Length);
            writer.Write(authorBytes);
                    
            var titleBytes = Encoding.ASCII.GetBytes(article.Title);
            writer.Write((short)titleBytes.Length);
            writer.Write(titleBytes);
                    
            var contentBytes = Encoding.ASCII.GetBytes(article.Content);
            writer.Write(contentBytes.Length);
            writer.Write(contentBytes);
                    
            writer.Write(article.Flags);
        }
    }
}
using System.Collections.Generic;

namespace DarkAges.Library.IO;

public class StringTable
{
    private readonly Dictionary<int, string> _strings = new Dictionary<int, string>();

    public StringTable(string filePath)
    {
        var tableFile = new TableFile(filePath);
        foreach (var row in tableFile.Rows)
        {
            if (row.Length >= 2 && int.TryParse(row[0], out var id))
            {
                _strings[id] = row[1].Replace("_", " ");
            }
        }
    }

    public string? GetString(int id)
    {
        return _strings.TryGetValue(id, out var value) ? value : null;
    }
}
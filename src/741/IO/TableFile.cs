using System.Collections.Generic;
using System.IO;

namespace DarkAges.Library.IO;

public class TableFile
{
    public List<string[]> Rows { get; }

    public TableFile(string filePath)
    {
        Rows = [];
        Load(filePath);
    }

    private void Load(string filePath)
    {
        if (!File.Exists(filePath)) return;

        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            Rows.Add(line.Split(['\t', ' '], System.StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
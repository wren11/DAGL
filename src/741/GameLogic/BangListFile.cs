using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DarkAges.Library.GameLogic;

public class BangListFile
{
    private readonly List<BangListEntry> _entries = [];

    public void Load(string filePath)
    {
        _entries.Clear();
        if (!File.Exists(filePath))
        {
            return;
        }

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split([' ', '\t'], 3, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                _entries.Add(new BangListEntry(parts[0], parts[1], parts[2]));
            }
        }
    }

    public void Save(string filePath)
    {
        var lines = _entries.Select(e => $"{e.Name} {e.Reason} {e.GmName}");
        File.WriteAllLines(filePath, lines);
    }

    public void AddEntry(string name, string reason, string gmName)
    {
        var entry = new BangListEntry(name, reason, gmName);
        _entries.Add(entry);
    }

    public bool RemoveEntry(string name)
    {
        var entryToRemove = _entries.FirstOrDefault(e => e.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        if (entryToRemove != null)
        {
            _entries.Remove(entryToRemove);
            return true;
        }
        return false;
    }

    public BangListEntry GetEntry(string name)
    {
        return _entries.FirstOrDefault(e => e.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<BangListEntry> GetAllEntries()
    {
        return _entries;
    }
}
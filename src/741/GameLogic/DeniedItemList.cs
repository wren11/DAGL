using System.Collections.Generic;

namespace DarkAges.Library.GameLogic;

public class DeniedItemList
{
    private readonly List<string> _deniedItems = [];
    private readonly List<string> _deniedSkills = [];
    private readonly List<string> _deniedSpells = [];

    public void AddItem(string item)
    {
        if (!_deniedItems.Contains(item))
            _deniedItems.Add(item);
    }

    public void AddSkill(string skill)
    {
        if (!_deniedSkills.Contains(skill))
            _deniedSkills.Add(skill);
    }

    public void AddSpell(string spell)
    {
        if (!_deniedSpells.Contains(spell))
            _deniedSpells.Add(spell);
    }

    public bool IsItemDenied(string item)
    {
        return _deniedItems.Contains(item);
    }

    public bool IsSkillDenied(string skill)
    {
        return _deniedSkills.Contains(skill);
    }

    public bool IsSpellDenied(string spell)
    {
        return _deniedSpells.Contains(spell);
    }

    public void LoadFromFile(string filePath)
    {
        try
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("item:"))
                {
                    AddItem(line.Substring(5).Trim());
                }
                else if (line.StartsWith("skill:"))
                {
                    AddSkill(line.Substring(6).Trim());
                }
                else if (line.StartsWith("spell:"))
                {
                    AddSpell(line.Substring(6).Trim());
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error loading denied item list: {ex.Message}");
        }
    }

    public void Clear()
    {
        _deniedItems.Clear();
        _deniedSkills.Clear();
        _deniedSpells.Clear();
    }
}
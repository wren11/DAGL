using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DarkAges.Library.Common.DataStructures;

namespace DarkAges.Library.IO;

public class LayoutFileParser
{
    private readonly RedBlackTree<string, RedBlackTree<string, string>> _sections = new RedBlackTree<string, RedBlackTree<string, string>>(System.StringComparer.OrdinalIgnoreCase);
    private string _currentSection;

    public LayoutFileParser(string filePath)
    {
        if (!File.Exists(filePath))
        {
            var defaultSection = new RedBlackTree<string, string>(System.StringComparer.OrdinalIgnoreCase);
            _sections.Insert("Default", defaultSection);
            _currentSection = "Default";
            return;
        }

        ParseLines(File.ReadAllLines(filePath));
    }
        
    public LayoutFileParser(string[] lines)
    {
        ParseLines(lines);
    }
    
    public static LayoutFileParser Parse(string filePath)
    {
        return new LayoutFileParser(filePath);
    }

    private void ParseLines(string[] lines)
    {
        _currentSection = "Default";
        var defaultSection = new RedBlackTree<string, string>(System.StringComparer.OrdinalIgnoreCase);
        _sections.Insert(_currentSection, defaultSection);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                continue;

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                var sectionName = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();
                _currentSection = sectionName;
                if (!_sections.TryGetValue(_currentSection, out _))
                {
                    _sections.Insert(_currentSection, new RedBlackTree<string, string>(System.StringComparer.OrdinalIgnoreCase));
                }
            }
            else
            {
                var parts = trimmedLine.Split(['='], 2);
                if (parts.Length == 2)
                {
                    if (_sections.TryGetValue(_currentSection, out var section))
                    {
                        section.Insert(parts[0].Trim(), parts[1].Trim());
                    }
                }
            }
        }
        SetSection("Default");
    }

    public void SetSection(string sectionName)
    {
        if (!_sections.TryGetValue(sectionName, out _))
            _sections.Insert(sectionName, new RedBlackTree<string, string>(System.StringComparer.OrdinalIgnoreCase));
            
        _currentSection = sectionName;
    }

    public bool HasKey(string key)
    {
        if (_sections.TryGetValue(_currentSection, out var section))
        {
            return section.TryGetValue(key, out _);
        }
        return false;
    }

    public string GetString(string key, string defaultValue = "")
    {
        if (_sections.TryGetValue(_currentSection, out var section) && section.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }
        
    public Rectangle GetRect(string key, Rectangle defaultValue = default)
    {
        if (_sections.TryGetValue(_currentSection, out var section) && section.TryGetValue(key, out var value))
        {
            var parts = value.Split(',');
            if (parts.Length == 4 && 
                int.TryParse(parts[0], out var x) &&
                int.TryParse(parts[1], out var y) &&
                int.TryParse(parts[2], out var width) &&
                int.TryParse(parts[3], out var height))
            {
                return new Rectangle(x, y, width, height);
            }
        }
        return defaultValue;
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        if (_sections.TryGetValue(_currentSection, out var section) && section.TryGetValue(key, out var value) && int.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    public float GetFloat(string key, float defaultValue = 0f)
    {
        if (_sections.TryGetValue(_currentSection, out var section) && section.TryGetValue(key, out var value) && float.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (_sections.TryGetValue(_currentSection, out var section) && section.TryGetValue(key, out var value) && bool.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    public Color GetColor(string key, Color defaultValue = default)
    {
        if (_sections.TryGetValue(_currentSection, out var section) && section.TryGetValue(key, out var value))
        {
            var parts = value.Split(',');
            if (parts.Length == 3 &&
                int.TryParse(parts[0], out var r) &&
                int.TryParse(parts[1], out var g) &&
                int.TryParse(parts[2], out var b))
            {
                return Color.FromArgb(r, g, b);
            }
            else if (parts.Length == 4 &&
                int.TryParse(parts[0], out var a) &&
                int.TryParse(parts[1], out var r2) &&
                int.TryParse(parts[2], out var g2) &&
                int.TryParse(parts[3], out var b2))
            {
                return Color.FromArgb(a, r2, g2, b2);
            }
            else
            {
                return Color.FromName(value);
            }
        }
        return defaultValue;
    }

    public RedBlackTree<string, string> GetSection(string sectionName)
    {
        if (_sections.TryGetValue(sectionName, out var section))
        {
            return section;
        }
        return null;
    }
}

public class Layout
{
    public RedBlackTree<string, RedBlackTree<string, string>> Sections { get; set; }

    public Layout()
    {
        Sections = new RedBlackTree<string, RedBlackTree<string, string>>(System.StringComparer.OrdinalIgnoreCase);
    }
    public void AddSection(string sectionName, RedBlackTree<string, string> section)
    {
        Sections.Insert(sectionName, section);
    }
    public RedBlackTree<string, string> GetSection(string sectionName)
    {
        if (Sections.TryGetValue(sectionName, out var section))
        {
            return section;
        }
        return null;
    }

    public int GetInt(string itemheight, int i)
    {
        if (Sections.TryGetValue("Default", out var section) && section.TryGetValue(itemheight, out var value) && int.TryParse(value, out var result))
        {
            return result;
        }
        return i; // Return default value if not found or parse fails
    }

    public string GetString(string key, string defaultValue = "")
    {
        if (Sections.TryGetValue("Default", out var section) && section.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }

    public Rectangle GetRect(string key, Rectangle defaultValue = default)
    {
        if (Sections.TryGetValue("Default", out var section) && section.TryGetValue(key, out var value))
        {
            var parts = value.Split(',');
            if (parts.Length == 4 &&
                int.TryParse(parts[0], out var x) &&
                int.TryParse(parts[1], out var y) &&
                int.TryParse(parts[2], out var width) &&
                int.TryParse(parts[3], out var height))
            {
                return new Rectangle(x, y, width, height);
            }
        }
        return defaultValue;
    }


    public Color GetColor(string backgroundcolor, Color white)
    {
        if (Sections.TryGetValue("Default", out var section) && section.TryGetValue(backgroundcolor, out var value))
        {
            var parts = value.Split(',');
            if (parts.Length == 3 &&
                int.TryParse(parts[0], out var r) &&
                int.TryParse(parts[1], out var g) &&
                int.TryParse(parts[2], out var b))
            {
                return Color.FromArgb(r, g, b);
            }
            else if (parts.Length == 4 &&
                int.TryParse(parts[0], out var a) &&
                int.TryParse(parts[1], out var r2) &&
                int.TryParse(parts[2], out var g2) &&
                int.TryParse(parts[3], out var b2))
            {
                return Color.FromArgb(a, r2, g2, b2);
            }
            else
            {
                return Color.FromName(value);
            }
        }
        return white; // Return default color if not found or parse fails
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (Sections.TryGetValue("Default", out var section) && section.TryGetValue(key, out var value) && bool.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    public float GetFloat(string key, float defaultValue = 0f)
    {
        if (Sections.TryGetValue("Default", out var section) && section.TryGetValue(key, out var value) && float.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    public bool HasKey(string key)
    {
        if (Sections.TryGetValue("Default", out var section))
        {
            return section.TryGetValue(key, out _);
        }
        return false;
    }

    public void SetSection(string sectionName)
    {
        if (!Sections.TryGetValue(sectionName, out _))
        {
            Sections.Insert(sectionName, new RedBlackTree<string, string>(System.StringComparer.OrdinalIgnoreCase));
        }
    }


}
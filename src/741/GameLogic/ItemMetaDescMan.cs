using DarkAges.Library.Common.DataStructures;

namespace DarkAges.Library.GameLogic;

public class ItemMetaDescMan
{
    private RedBlackTree<short, ItemMetaDesc> _descriptions = new RedBlackTree<short, ItemMetaDesc>();

    public void Load(string filePath)
    {
        try
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('\t');
                if (parts.Length == 2 && short.TryParse(parts[0], out var id))
                {
                    _descriptions.Add(id, new ItemMetaDesc { Id = id, Name = parts[1] });
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error loading item meta descriptions: {ex.Message}");
        }
    }

    public string GetDescription(short id)
    {
        if (_descriptions.TryGetValue(id, out var desc))
        {
            return desc.Name;
        }

        return null;
    }

    public void Clear()
    {
        _descriptions.Clear();
    }
}
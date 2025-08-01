using System.Collections.Generic;

namespace DarkAges.Library.Graphics;

public class ColoringTableMan
{
    private readonly Dictionary<string, ColoringTable> _tables = new Dictionary<string, ColoringTable>();
        
    public ColoringTable GetTable(string name, int tupleSize)
    {
        if (!_tables.ContainsKey(name))
        {
            _tables[name] = new ColoringTable(new List<ColoringTuple>());
        }
        return _tables[name];
    }
        
    public ColoringTable FindTable(string name)
    {
        _tables.TryGetValue(name, out var table);
        return table;
    }
}
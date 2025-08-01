using System.Collections.Generic;

namespace DarkAges.Library.Graphics;

public class ColoringTable
{
    public readonly List<ColoringTuple> Tuples = new List<ColoringTuple>();

    public ColoringTable(List<ColoringTuple> tuples)
    {
        Tuples = tuples ?? new List<ColoringTuple>();
    }

    public ColoringTable(byte[] mapping)
    {
        if (mapping == null) return;
        for (int i = 0; i < mapping.Length; i++)
        {
            Tuples.Add(new ColoringTuple(i, new short[] { mapping[i] }));
        }
    }

    public byte GetColor(byte originalColor)
    {
        foreach (var tuple in Tuples)
        {
            if (tuple.OriginalColor == originalColor)
            {
                return (byte)tuple.Colors[0];
            }
        }
        return originalColor; // Return original color if no match found
    }
}
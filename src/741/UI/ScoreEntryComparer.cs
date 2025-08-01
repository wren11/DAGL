namespace DarkAges.Library.UI;

/// <summary>
/// Comparer for sorting score entries (highest score first)
/// </summary>
public class ScoreEntryComparer : System.Collections.IComparer
{
    public int Compare(object x, object y)
    {
        var a = (ScoreEntry)x;
        var b = (ScoreEntry)y;
            
        // Sort by score (descending), then by timestamp (ascending)
        var scoreComparison = b.Score.CompareTo(a.Score);
        if (scoreComparison != 0)
            return scoreComparison;
                
        return a.Timestamp.CompareTo(b.Timestamp);
    }
}
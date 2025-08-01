using System.Drawing;

namespace DarkAges.Library.UI;

public class GameMessagePane
{
    private int _fontId;
    private Rectangle _bounds;
    private int _lineHeight;
    private int _maxLines;
    private int _maxColumns;

    private string[][] _lines; 
    private int[] _lineWidths;
    private int[] _lineColors;

    private int _currentLine;
    private int _topLine;
    private int _scrollOffset;

    public GameMessagePane(int fontId, Rectangle bounds)
    {
        _fontId = fontId;
        _bounds = bounds;

        _lineHeight = 14; 
        _maxLines = bounds.Height / _lineHeight;
        _maxColumns = bounds.Width / 6; 

        _lines = new string[_maxLines][];
        _lineWidths = new int[_maxLines];
        _lineColors = new int[_maxLines];
        for (var i = 0; i < _maxLines; i++)
        {
            _lines[i] = new string[1];
            _lines[i][0] = string.Empty;
        }
    }
}
namespace DarkAges.Library.UI.ItemShop;

public class IconInvControlPane(int rows, int columns, object[] items) : ControlPane
{
    private int _selectedSlot = -1;
    private int _itemCount = items?.Length ?? 0;
    private readonly object[] _items = items; // In a real implementation, this would be a specific item type

    public void SetSelection(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= columns)
        {
            _selectedSlot = -1;
            return;
        }
            
        var index = row * columns + col;
        if (index < _itemCount)
        {
            _selectedSlot = index;
        }
        else
        {
            _selectedSlot = -1;
        }
    }
        
    // Render and event handling logic would be implemented here
}
using DarkAges.Library.GameLogic;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI;

public class InventoryPane_A : ControlPane
{
    private readonly List<ItemPane> _slots = new List<ItemPane>(60);
    private int _selectedSlot = -1;
    private User _user;
        
    private int _columns = 12;
    private int _rowsDisplayed = 3;
    private int _slotWidth = 33;
    private int _slotHeight = 36;
    private Point _gridOffset = new Point(4, 2);

    public InventoryPane_A(User user)
    {
        _user = user;
        for (var i = 0; i < 60; i++)
        {
            var itemPane = new ItemPane(null, null, new Rectangle());
            _slots.Add(itemPane);
            AddChild(itemPane);
        }
        UpdateLayout();
    }
        
    public void SetUser(User user)
    {
        _user = user;
        UpdateInventory();
    }

    public void UpdateInventory()
    {
        if (_user == null) return;
        for (var i = 0; i < 60; i++)
        {
            if (i < _user.Inventory.Count)
            {
                var item = _user.Inventory[i];
                _slots[i].ItemId = (short)item.Id;
                _slots[i].ItemName = item.Name;
            }
            else
            {
                _slots[i].ItemId = 0;
                _slots[i].ItemName = "";
            }
        }
    }

    private void UpdateLayout()
    {
        for (var i = 0; i < 60; i++)
        {
            var row = i / _columns;
            var col = i % _columns;
            _slots[i].Position = new Point(Position.X + _gridOffset.X + col * _slotWidth, Position.Y + _gridOffset.Y + row * _slotHeight);
            _slots[i].Size = new DarkAges.Library.Graphics.Size(_slotWidth, _slotHeight);
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (base.HandleEvent(e)) return true;

        if (e is MouseEvent me && me.Type == EventType.LButtonDown)
        {
            if (Bounds.Contains(me.X, me.Y))
            {
                var localX = me.X - Position.X - _gridOffset.X;
                var localY = me.Y - Position.Y - _gridOffset.Y;

                var col = localX / _slotWidth;
                var row = localY / _slotHeight;

                if (col >= 0 && col < _columns && row >= 0 && row < _rowsDisplayed)
                {
                    var index = row * _columns + col;
                    if (index < _slots.Count)
                    {
                        _selectedSlot = index;
                        // In a real implementation, this would likely fire an event
                        // or send a network packet (sub_490F40).
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;
            
        base.Render(spriteBatch);

        if (_selectedSlot != -1)
        {
            var slot = _slots[_selectedSlot];
            spriteBatch.DrawRectangle(new Rectangle(slot.Position.X, slot.Position.Y, slot.Size.Width, slot.Size.Height), ColorRgb565.Yellow);
        }
    }
}
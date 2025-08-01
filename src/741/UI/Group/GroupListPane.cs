using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Group;

public class GroupListPane : ControlPane
{
    private readonly List<GroupInfo> _groups = [];
    private readonly List<TextButtonExControlPane> _groupButtons = [];
    private int _selectedIndex = -1;

    public event EventHandler<GroupInfo> GroupSelected;

    public void AddGroup(GroupInfo group)
    {
        _groups.Add(group);
        var button = new TextButtonExControlPane($"{group.Name} ({group.MemberCount}/{group.MaxMembers})");
        button.Position = new Point(50, 100 + _groups.Count * 30);
        button.Click += (s, e) => SelectGroup(_groups.Count - 1);
        _groupButtons.Add(button);
        AddChild(button);
    }

    public void RemoveGroup(GroupInfo group)
    {
        var index = _groups.IndexOf(group);
        if (index >= 0)
        {
            _groups.RemoveAt(index);
            if (index < _groupButtons.Count)
            {
                RemoveChild(_groupButtons[index]);
                _groupButtons.RemoveAt(index);
            }
        }
    }

    private void SelectGroup(int index)
    {
        _selectedIndex = index;
        if (index >= 0 && index < _groups.Count)
        {
            GroupSelected?.Invoke(this, _groups[index]);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Groups", 50, 50, Color.Black);
            
        base.Render(spriteBatch);
    }
}
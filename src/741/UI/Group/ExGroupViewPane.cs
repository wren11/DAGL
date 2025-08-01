using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Group;

public class ExGroupViewPane : ControlPane
{
    private GroupInfo _currentGroup;
    private TextButtonExControlPane _joinButton;
    private TextButtonExControlPane _leaveButton;

    public ExGroupViewPane()
    {
        _joinButton = new TextButtonExControlPane("Join Group");
        _leaveButton = new TextButtonExControlPane("Leave Group");

        _joinButton.Position = new Point(500, 100);
        _leaveButton.Position = new Point(500, 130);

        AddChild(_joinButton);
        AddChild(_leaveButton);
    }

    public void SetGroup(GroupInfo group)
    {
        _currentGroup = group;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        //spriteBatch.DrawString(font, "Group Details", 500, 50, Color.Black);

        if (_currentGroup != null)
        {
            //spriteBatch.DrawString(font, $"Name: {_currentGroup.Name}", 500, 80, Color.Black);
            //spriteBatch.DrawString(font, $"Members: {_currentGroup.MemberCount}/{_currentGroup.MaxMembers}", 500, 100, Color.Black);
            //spriteBatch.DrawString(font, $"Leader: {_currentGroup.LeaderName}", 500, 120, Color.Black);
        }
            
        base.Render(spriteBatch);
    }
}
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Users;

public class ShowUsersPane_A : ControlPane
{
    private ShowUsersListPane _usersListPane;
    private readonly List<UserInfo> _onlineUsers = [];

    public event EventHandler<UserInfo> UserSelected;

    public ShowUsersPane_A()
    {
        _usersListPane = new ShowUsersListPane();
        _usersListPane.UserSelected += (s, user) => HandleUserSelected(user);
    }

    public void ShowUsersList()
    {
        _usersListPane.Show();
    }

    private void HandleUserSelected(UserInfo user)
    {
        UserSelected?.Invoke(this, user);
    }

    public void AddOnlineUser(UserInfo user)
    {
        _onlineUsers.Add(user);
        _usersListPane.AddUser(user);
    }

    public void RemoveOnlineUser(UserInfo user)
    {
        _onlineUsers.Remove(user);
        _usersListPane.RemoveUser(user);
    }

    public void UpdateUserStatus(int userId, bool isOnline, int level, string className)
    {
        _usersListPane.UpdateUserStatus(userId, isOnline, level, className);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _usersListPane.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_usersListPane.HandleEvent(e)) return true;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        _usersListPane?.Dispose();
    }
}
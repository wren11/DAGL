using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Friends;

public class FriendSystem : ControlPane
{
    private FriendListDialog _friendListDialog;
    private readonly List<FriendEntry> _friends = [];

    public event EventHandler<FriendEntry> FriendStatusChanged;

    public FriendSystem()
    {
        _friendListDialog = new FriendListDialog();
        _friendListDialog.FriendAdded += (s, friend) => AddFriend(friend);
        _friendListDialog.FriendRemoved += (s, friend) => RemoveFriend(friend);
        _friendListDialog.FriendWhispered += (s, friend) => WhisperFriend(friend);
    }

    public void ShowFriendList()
    {
        _friendListDialog.Show();
    }

    public void AddFriend(FriendEntry friend)
    {
        _friends.Add(friend);
    }

    public void RemoveFriend(FriendEntry friend)
    {
        _friends.RemoveAll(f => f.Id == friend.Id);
    }

    public void WhisperFriend(FriendEntry friend)
    {
        var whisperEvent = new WhisperEvent(friend.Name, "");
        EventManager.Instance.DispatchEvent(whisperEvent);
    }

    public void UpdateFriendStatus(int friendId, bool isOnline, int level, string className)
    {
        var friend = _friends.Find(f => f.Id == friendId);
        if (friend != null)
        {
            friend.IsOnline = isOnline;
            friend.Level = level;
            friend.Class = className;
            friend.LastSeen = DateTime.Now;
                
            _friendListDialog.UpdateFriendStatus(friendId, isOnline, level, className);
            FriendStatusChanged?.Invoke(this, friend);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _friendListDialog.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_friendListDialog.HandleEvent(e)) return true;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        _friendListDialog?.Dispose();
    }
}
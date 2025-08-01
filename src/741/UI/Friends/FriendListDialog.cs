using System;
using System.Collections.Generic;
using DarkAges.Library.Graphics;
using System.Drawing;
using Size = DarkAges.Library.Graphics.Size;

namespace DarkAges.Library.UI.Friends;

public class FriendListDialog : ControlPane
{
    private readonly List<FriendEntry> _friends = [];
    private readonly List<TextButtonExControlPane> _friendButtons = [];
    private TextButtonExControlPane _addButton;
    private TextButtonExControlPane _removeButton;
    private TextButtonExControlPane _whisperButton;
    private TextButtonExControlPane _closeButton;
    private FriendEntry _selectedFriend;
    private TextEditControlPane _searchBox;

    public event EventHandler<FriendEntry> FriendAdded;
    public event EventHandler<FriendEntry> FriendRemoved;
    public event EventHandler<FriendEntry> FriendWhispered;

    public FriendListDialog()
    {
        InitializeControls();
        LoadFriends();
    }

    private void InitializeControls()
    {
        _addButton = new TextButtonExControlPane("Add Friend");
        _removeButton = new TextButtonExControlPane("Remove Friend");
        _whisperButton = new TextButtonExControlPane("Whisper");
        _closeButton = new TextButtonExControlPane("Close");
        _searchBox = new TextEditControlPane();

        _addButton.Position = new Point(50, 350);
        _removeButton.Position = new Point(150, 350);
        _whisperButton.Position = new Point(250, 350);
        _closeButton.Position = new Point(350, 350);
        _searchBox.Position = new Point(50, 50);
        _searchBox.Size = new Size(200, 20);

        _addButton.Click += (s, e) => AddFriend();
        _removeButton.Click += (s, e) => RemoveFriend();
        _whisperButton.Click += (s, e) => WhisperFriend();
        _closeButton.Click += (s, e) => Hide();
        _searchBox.TextChanged += (s, e) => FilterFriends();

        AddChild(_addButton);
        AddChild(_removeButton);
        AddChild(_whisperButton);
        AddChild(_closeButton);
        AddChild(_searchBox);
    }

    private void LoadFriends()
    {
        _friends.Add(new FriendEntry { Id = 1, Name = "Player1", IsOnline = true, Level = 50, Class = "Warrior" });
        _friends.Add(new FriendEntry { Id = 2, Name = "Player2", IsOnline = false, Level = 45, Class = "Mage" });
        _friends.Add(new FriendEntry { Id = 3, Name = "Player3", IsOnline = true, Level = 60, Class = "Priest" });

        UpdateFriendButtons();
    }

    private void UpdateFriendButtons()
    {
        foreach (var button in _friendButtons)
        {
            RemoveChild(button);
        }
        _friendButtons.Clear();

        for (var i = 0; i < _friends.Count; i++)
        {
            var friend = _friends[i];
            var status = friend.IsOnline ? "Online" : "Offline";
            var button = new TextButtonExControlPane($"{friend.Name} ({friend.Level} {friend.Class}) - {status}");
            button.Position = new Point(50, 100 + i * 30);
            button.Click += (s, e) => SelectFriend(friend);
            _friendButtons.Add(button);
            AddChild(button);
        }
    }

    private void SelectFriend(FriendEntry friend)
    {
        _selectedFriend = friend;
    }

    private void AddFriend()
    {
        var friendName = _searchBox.Text.Trim();
        if (!string.IsNullOrEmpty(friendName))
        {
            var friend = new FriendEntry
            {
                Id = _friends.Count + 1,
                Name = friendName,
                IsOnline = false,
                Level = 0,
                Class = "Unknown"
            };

            _friends.Add(friend);
            UpdateFriendButtons();
            FriendAdded?.Invoke(this, friend);
            _searchBox.Text = "";
        }
    }

    private void RemoveFriend()
    {
        if (_selectedFriend != null)
        {
            _friends.Remove(_selectedFriend);
            UpdateFriendButtons();
            FriendRemoved?.Invoke(this, _selectedFriend);
            _selectedFriend = null;
        }
    }

    private void WhisperFriend()
    {
        if (_selectedFriend != null)
        {
            var whisperEvent = new WhisperEvent(_selectedFriend.Name, "");
            FriendWhispered?.Invoke(this, _selectedFriend);
        }
    }

    private void FilterFriends()
    {
        var searchText = _searchBox.Text.ToLower();
        var filteredFriends = _friends.FindAll(f => f.Name.ToLower().Contains(searchText));
            
        foreach (var button in _friendButtons)
        {
            RemoveChild(button);
        }
        _friendButtons.Clear();

        for (var i = 0; i < filteredFriends.Count; i++)
        {
            var friend = filteredFriends[i];
            var status = friend.IsOnline ? "Online" : "Offline";
            var button = new TextButtonExControlPane($"{friend.Name} ({friend.Level} {friend.Class}) - {status}");
            button.Position = new Point(50, 100 + i * 30);
            button.Click += (s, e) => SelectFriend(friend);
            _friendButtons.Add(button);
            AddChild(button);
        }
    }

    public void UpdateFriendStatus(int friendId, bool isOnline, int level, string className)
    {
        var friend = _friends.Find(f => f.Id == friendId);
        if (friend != null)
        {
            friend.IsOnline = isOnline;
            friend.Level = level;
            friend.Class = className;
            UpdateFriendButtons();
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        var backgroundRect = new Rectangle(30, 30, 450, 400);
        spriteBatch.DrawRectangle(backgroundRect, Color.White);
        spriteBatch.DrawRectangle(backgroundRect, Color.Black);

        //spriteBatch.DrawString(font, "Friend List", 200, 30, Color.Black);
        //spriteBatch.DrawString(font, "Search:", 50, 80, Color.Black);
            
        base.Render(spriteBatch);
    }
}
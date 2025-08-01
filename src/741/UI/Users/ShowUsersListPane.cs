using System;
using System.Collections.Generic;
using DarkAges.Library.Graphics;
using System.Drawing;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI.Users;

public class ShowUsersListPane : ControlPane
{
    private readonly List<UserInfo> _users = [];
    private readonly List<TextButtonExControlPane> _userButtons = [];
    private TextButtonExControlPane _refreshButton;
    private TextButtonExControlPane _closeButton;
    private TextEditControlPane _searchBox;
    private ImagePane _backgroundImage;
    private Rectangle _backgroundRect;
    private UserInfo _selectedUser;
    private bool _isRefreshing;

    public event EventHandler<UserInfo> UserSelected;
    public event EventHandler RefreshRequested;

    public ShowUsersListPane()
    {
        InitializeControls();
        LoadLayout();
    }

    private void InitializeControls()
    {
        _refreshButton = new TextButtonExControlPane("Refresh");
        _closeButton = new TextButtonExControlPane("Close");
        _searchBox = new TextEditControlPane();

        _refreshButton.Position = new Point(50, 350);
        _closeButton.Position = new Point(150, 350);
        _searchBox.Position = new Point(50, 50);
        _searchBox.Size = new System.Drawing.Size(200, 20);

        _refreshButton.Click += (s, e) => RefreshUsers();
        _closeButton.Click += (s, e) => Hide();
        _searchBox.TextChanged += (s, e) => FilterUsers();

        AddChild(_refreshButton);
        AddChild(_closeButton);
        AddChild(_searchBox);
    }

    private void Hide()
    {
        IsVisible = false;
        OnClosed();
    }

    private void OnClosed()
    {
        // Do nothing
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_userslistdlg.txt");
                
            _backgroundImage = null;
            var backgroundName = layout.GetString("Background", "users_bg");
            var indexedImage = ImageLoader.LoadImage(backgroundName);
            if (indexedImage != null)
            {
                _backgroundImage = new ImagePane();
                _backgroundImage.SetImage(indexedImage, null);
            }

            var backgroundRect = layout.GetRect("Background");
            _backgroundRect = backgroundRect;
                
            var refreshRect = layout.GetRect("Refresh");
            var closeRect = layout.GetRect("Close");
            var searchRect = layout.GetRect("Search");
                
            _refreshButton.Bounds = refreshRect;
            _closeButton.Bounds = closeRect;
            _searchBox.Bounds = searchRect;
        }
        catch
        {
            _backgroundRect = new Rectangle(50, 50, 400, 400);
        }
    }

    private void UpdateUserButtons()
    {
        foreach (var button in _userButtons)
        {
            RemoveChild(button);
        }
        _userButtons.Clear();

        for (var i = 0; i < _users.Count; i++)
        {
            var user = _users[i];
            var status = user.IsOnline ? "Online" : "Offline";
            var button = new TextButtonExControlPane($"{user.Name} (Lv.{user.Level} {user.Class}) - {status}");
            button.Position = new Point(50, 100 + i * 30);
            button.Click += (s, e) => SelectUser(user);
            _userButtons.Add(button);
            AddChild(button);
        }
    }

    private void SelectUser(UserInfo user)
    {
        _selectedUser = user;
        UserSelected?.Invoke(this, user);
    }

    private void RefreshUsers()
    {
        if (_isRefreshing) return;

        _isRefreshing = true;
        _refreshButton.Text = "Refreshing...";
        _refreshButton.Enabled = false;

        RefreshRequested?.Invoke(this, EventArgs.Empty);

        _isRefreshing = false;
        _refreshButton.Text = "Refresh";
        _refreshButton.Enabled = true;
    }

    private void FilterUsers()
    {
        var searchText = _searchBox.Text.ToLower();
        var filteredUsers = _users.FindAll(u => u.Name.ToLower().Contains(searchText));
            
        foreach (var button in _userButtons)
        {
            RemoveChild(button);
        }
        _userButtons.Clear();

        for (var i = 0; i < filteredUsers.Count; i++)
        {
            var user = filteredUsers[i];
            var status = user.IsOnline ? "Online" : "Offline";
            var button = new TextButtonExControlPane($"{user.Name} (Lv.{user.Level} {user.Class}) - {status}");
            button.Position = new Point(50, 100 + i * 30);
            button.Click += (s, e) => SelectUser(user);
            _userButtons.Add(button);
            AddChild(button);
        }
    }

    public void AddUser(UserInfo user)
    {
        _users.Add(user);
        UpdateUserButtons();
    }

    public void RemoveUser(UserInfo user)
    {
        _users.Remove(user);
        UpdateUserButtons();
    }

    public void UpdateUserStatus(int userId, bool isOnline, int level, string className)
    {
        var user = _users.Find(u => u.Id == userId);
        if (user != null)
        {
            user.IsOnline = isOnline;
            user.Level = level;
            user.Class = className;
            UpdateUserButtons();
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        if (_backgroundImage != null)
        {
            _backgroundImage.Render(spriteBatch);
        }
        else
        {
            spriteBatch.DrawRectangle(_backgroundRect, Color.White);
            spriteBatch.DrawRectangle(_backgroundRect, Color.Black);
        }

        base.Render(spriteBatch);
    }

    internal void Show()
    {
        IsVisible = true;
    }
}
using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Users;

public class LoadUsersPane : ControlPane
{
    private readonly List<UserInfo> _savedUsers = [];
    private readonly List<TextButtonExControlPane> _userButtons = [];
    private TextButtonExControlPane _loadButton;
    private TextButtonExControlPane _deleteButton;
    private TextButtonExControlPane _closeButton;
    private UserInfo _selectedUser;

    public event EventHandler<UserInfo> UserLoadRequested;
    public event EventHandler<UserInfo> UserDeleteRequested;

    public LoadUsersPane()
    {
        InitializeControls();
    }

    private void InitializeControls()
    {
        _loadButton = new TextButtonExControlPane("Load");
        _deleteButton = new TextButtonExControlPane("Delete");
        _closeButton = new TextButtonExControlPane("Close");

        _loadButton.Position = new Point(50, 350);
        _deleteButton.Position = new Point(150, 350);
        _closeButton.Position = new Point(250, 350);

        _loadButton.Click += (s, e) => LoadUser();
        _deleteButton.Click += (s, e) => DeleteUser();
        _closeButton.Click += (s, e) => Hide();

        AddChild(_loadButton);
        AddChild(_deleteButton);
        AddChild(_closeButton);
    }

    private void Hide()
    {
        IsVisible = false;
    }

    private void UpdateUserButtons()
    {
        foreach (var button in _userButtons)
        {
            RemoveChild(button);
        }
        _userButtons.Clear();

        for (var i = 0; i < _savedUsers.Count; i++)
        {
            var user = _savedUsers[i];
            var button = new TextButtonExControlPane($"{user.Name} (Lv.{user.Level} {user.Class})");
            button.Position = new Point(50, 100 + i * 30);
            button.Click += (s, e) => SelectUser(user);
            _userButtons.Add(button);
            AddChild(button);
        }
    }

    private void SelectUser(UserInfo user)
    {
        _selectedUser = user;
    }

    private void LoadUser()
    {
        if (_selectedUser != null)
        {
            UserLoadRequested?.Invoke(this, _selectedUser);
            Hide();
        }
    }

    private void DeleteUser()
    {
        if (_selectedUser != null)
        {
            UserDeleteRequested?.Invoke(this, _selectedUser);
            _savedUsers.Remove(_selectedUser);
            UpdateUserButtons();
            _selectedUser = null;
        }
    }

    public void AddSavedUser(UserInfo user)
    {
        _savedUsers.Add(user);
        UpdateUserButtons();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var backgroundRect = new Rectangle(50, 50, 400, 400);
        spriteBatch.DrawRectangle(backgroundRect, Color.White);
        spriteBatch.DrawRectangle(backgroundRect, Color.Black);

        base.Render(spriteBatch);
    }
}
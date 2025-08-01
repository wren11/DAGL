using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NexonClub;

public class NexonClubSystem : ControlPane
{
    private NexonClubAuthAlertPane _authAlertPane = null!;
    private NexonclubDialog _mainDialog = null!;
    private bool _isAuthenticated;

    public event EventHandler<bool> AuthenticationChanged = delegate { };

    public NexonClubSystem()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _authAlertPane = new NexonClubAuthAlertPane();
        _mainDialog = new NexonclubDialog();

        _authAlertPane.AuthenticationRequested += (s, e) => RequestAuthentication();
        _mainDialog.LoginCompleted += (s, success) => HandleLoginResult(success);
    }

    public void ShowAuthAlert()
    {
        _authAlertPane.Show();
    }

    public void ShowMainDialog()
    {
        _mainDialog.Show();
    }

    private void RequestAuthentication()
    {
        _authAlertPane.Hide();
        _mainDialog.Show();
    }

    private void HandleLoginResult(bool success)
    {
        _isAuthenticated = success;
        AuthenticationChanged?.Invoke(this, success);
            
        if (success)
        {
            _mainDialog.Hide();
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _authAlertPane.Render(spriteBatch);
        _mainDialog.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_authAlertPane.HandleEvent(e)) return true;
        if (_mainDialog.HandleEvent(e)) return true;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        _authAlertPane?.Dispose();
        _mainDialog?.Dispose();
    }
}
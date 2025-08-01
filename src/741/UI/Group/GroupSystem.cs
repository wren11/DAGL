using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Group;

public class GroupSystem : ControlPane
{
    private readonly List<GroupInfo> _groups = [];
    private GroupListPane _groupListPane;
    private GroupAdDialogPane _groupAdDialog;
    private GroupAdPane _groupAdPane;
    private ExGroupViewPane _exGroupViewPane;
    private GroupAdInfoDialogPane _groupAdInfoDialog;

    public event EventHandler<GroupInfo> GroupSelected;
    public event EventHandler<GroupInfo> GroupJoined;

    public GroupSystem()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _groupListPane = new GroupListPane();
        _groupAdDialog = new GroupAdDialogPane();
        _groupAdPane = new GroupAdPane();
        _exGroupViewPane = new ExGroupViewPane();
        _groupAdInfoDialog = new GroupAdInfoDialogPane();

        _groupListPane.GroupSelected += (s, group) => GroupSelected?.Invoke(this, group);
        _groupAdDialog.GroupJoined += (s, group) => GroupJoined?.Invoke(this, group);
    }

    public void AddGroup(GroupInfo group)
    {
        _groups.Add(group);
        _groupListPane.AddGroup(group);
    }

    public void RemoveGroup(GroupInfo group)
    {
        _groups.Remove(group);
        _groupListPane.RemoveGroup(group);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _groupListPane.Render(spriteBatch);
        _groupAdDialog.Render(spriteBatch);
        _groupAdPane.Render(spriteBatch);
        _exGroupViewPane.Render(spriteBatch);
        _groupAdInfoDialog.Render(spriteBatch);

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_groupListPane.HandleEvent(e)) return true;
        if (_groupAdDialog.HandleEvent(e)) return true;
        if (_groupAdPane.HandleEvent(e)) return true;
        if (_exGroupViewPane.HandleEvent(e)) return true;
        if (_groupAdInfoDialog.HandleEvent(e)) return true;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        _groupListPane?.Dispose();
        _groupAdDialog?.Dispose();
        _groupAdPane?.Dispose();
        _exGroupViewPane?.Dispose();
        _groupAdInfoDialog?.Dispose();
    }
}
using System;
using System.Drawing;
using DarkAges.Library.Core.Events;
using Silk.NET.GLFW;

namespace DarkAges.Library.UI;

public abstract class DialogPane : Pane
{
    public string Title { get; set; } = "";
    public new bool IsModal { get; set; } = true;
    public bool IsClosable { get; set; } = true;
    public bool IsMinimizable { get; set; } = false;
    public bool IsMaximizable { get; set; } = false;

    protected DialogPane()
    {
    }

    protected DialogPane(Rectangle bounds) : base(bounds)
    {
    }

    public void SetTitle(string title)
    {
        Title = title;
    }

    public void SetModal(bool isModal)
    {
        IsModal = isModal;
    }

    public virtual void Show()
    {
        IsVisible = true;
        OnShown();
    }

    public virtual void Hide()
    {
        IsVisible = false;
        OnClosed();
    }

    public virtual void Close()
    {
        if (IsClosable)
        {
            Hide();
        }
    }

    public virtual void Close(int result)
    {
        if (IsClosable)
        {
            Hide();
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !IsEnabled) return false;

        // Handle close events
        if (e is KeyEvent ke && (int)ke.Key == (int)Keys.Escape && IsClosable)
        {
            Close();
            return true;
        }

        return base.HandleEvent(e);
    }
}
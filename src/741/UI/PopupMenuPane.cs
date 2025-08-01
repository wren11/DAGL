using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

/// <summary>
/// Handles popup menu display and interaction
/// </summary>
public class PopupMenuPane : Pane
{
    private const int MAX_MENU_ITEMS = 10;
    private const int MENU_ITEM_HEIGHT = 20;
    private const int MENU_PADDING = 5;

    private List<MenuItem> menuItems;
    private int selectedIndex;
    private bool isVisible;
    private bool isModal;
    private int menuWidth;
    private int menuHeight;
    private Point menuPosition;
    private System.Drawing.Color backgroundColor;
    private System.Drawing.Color textColor;
    private System.Drawing.Color selectedColor;
    private System.Drawing.Color borderColor;
    private SimpleFont menuFont;
    private string title;
    private bool showTitle;
    private int titleHeight;

    // Animation properties
    private float fadeAlpha;
    private bool isFading;
    private float fadeSpeed;
    private bool fadeIn;

    // Event handlers
    public event Action<int> MenuItemSelected = delegate { };
    public event Action MenuClosed = delegate { };

    public PopupMenuPane()
    {
        InitializeMenu();
        LoadLayout();
    }

    private void InitializeMenu()
    {
        menuItems = [];
        selectedIndex = -1;
        isVisible = false;
        isModal = false;
        title = "";
        showTitle = false;
        titleHeight = 0;
        fadeAlpha = 0.0f;
        isFading = false;
        fadeIn = false;
        menuFont = null!;
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_popupmenu.txt");
            menuWidth = layout.GetInt("Width", 200);
            backgroundColor = layout.GetColor("BackgroundColor", Color.FromArgb(255, 64, 64, 64));
            textColor = layout.GetColor("TextColor", Color.White);
            selectedColor = layout.GetColor("SelectedColor", Color.FromArgb(255, 128, 128, 128));
            borderColor = layout.GetColor("BorderColor", Color.FromArgb(255, 192, 192, 192));
            var fontName = layout.GetString("FontName", "default");
            menuFont = FontManager.GetSimpleFont(fontName);
            fadeSpeed = layout.GetFloat("FadeSpeed", 0.1f);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading popup menu layout: {ex.Message}");
            // Fallback to defaults
            menuWidth = 200;
            backgroundColor = Color.FromArgb(255, 64, 64, 64);
            textColor = Color.White;
            selectedColor = Color.FromArgb(255, 128, 128, 128);
            borderColor = Color.FromArgb(255, 192, 192, 192);
            menuFont = FontManager.GetSimpleFont("default");
            fadeSpeed = 0.1f;
        }
    }

    public void AddMenuItem(string text, int id = -1, bool enabled = true)
    {
        if (menuItems.Count < MAX_MENU_ITEMS)
        {
            var item = new MenuItem
            {
                Text = text,
                Id = id,
                Enabled = enabled
            };
            menuItems.Add(item);
            UpdateMenuSize();
        }
    }

    public void AddMenuItem(string text, Action action, bool enabled = true)
    {
        if (menuItems.Count < MAX_MENU_ITEMS)
        {
            var item = new MenuItem
            {
                Text = text,
                Action = action,
                Enabled = enabled
            };
            menuItems.Add(item);
            UpdateMenuSize();
        }
    }

    public void ClearMenuItems()
    {
        menuItems.Clear();
        selectedIndex = -1;
        UpdateMenuSize();
    }

    public void SetTitle(string? title)
    {
        this.title = title ?? string.Empty;
        showTitle = !string.IsNullOrEmpty(title);
        UpdateMenuSize();
    }

    public void Show(Point position)
    {
        menuPosition = position;
        isVisible = true;
        selectedIndex = -1;
            
        // Start fade in animation
        fadeAlpha = 0.0f;
        isFading = true;
        fadeIn = true;
            
        // Update position
        SetPosition(position.X, position.Y);
            
        // Make sure menu is within screen bounds
        EnsureMenuInBounds();
    }

    public void Show(int x, int y)
    {
        Show(new Point(x, y));
    }

    public void Hide()
    {
        if (isVisible)
        {
            // Start fade out animation
            isFading = true;
            fadeIn = false;
        }
    }

    public void Close()
    {
        isVisible = false;
        selectedIndex = -1;
        MenuClosed?.Invoke();
    }

    private void UpdateMenuSize()
    {
        var totalHeight = 0;
            
        if (showTitle)
        {
            titleHeight = MENU_ITEM_HEIGHT + MENU_PADDING;
            totalHeight += titleHeight;
        }
            
        totalHeight += menuItems.Count * MENU_ITEM_HEIGHT;
        totalHeight += MENU_PADDING * 2;
            
        menuHeight = totalHeight;
        SetSize(menuWidth, menuHeight);
    }

    private void EnsureMenuInBounds()
    {
        // Get screen bounds (assuming 640x480 for now)
        var screenWidth = 640;
        var screenHeight = 480;
            
        if (menuPosition.X + menuWidth > screenWidth)
        {
            menuPosition.X = screenWidth - menuWidth;
        }
            
        if (menuPosition.Y + menuHeight > screenHeight)
        {
            menuPosition.Y = screenHeight - menuHeight;
        }
            
        if (menuPosition.X < 0) menuPosition.X = 0;
        if (menuPosition.Y < 0) menuPosition.Y = 0;
            
        SetPosition(menuPosition.X, menuPosition.Y);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
            
        if (isFading)
        {
            if (fadeIn)
            {
                fadeAlpha += fadeSpeed;
                if (fadeAlpha >= 1.0f)
                {
                    fadeAlpha = 1.0f;
                    isFading = false;
                }
            }
            else
            {
                fadeAlpha -= fadeSpeed;
                if (fadeAlpha <= 0.0f)
                {
                    fadeAlpha = 0.0f;
                    isFading = false;
                    Close();
                }
            }
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!isVisible || fadeAlpha <= 0.0f)
            return;

        // Apply fade alpha
        var currentBgColor = System.Drawing.Color.FromArgb((int)(backgroundColor.A * fadeAlpha), backgroundColor);
        var currentTextColor = System.Drawing.Color.FromArgb((int)(textColor.A * fadeAlpha), textColor);
        var currentSelectedColor = System.Drawing.Color.FromArgb((int)(selectedColor.A * fadeAlpha), selectedColor);
        var currentBorderColor = System.Drawing.Color.FromArgb((int)(borderColor.A * fadeAlpha), borderColor);

        // Draw background
        spriteBatch.FillRectangle(Bounds, currentBgColor);
            
        // Draw border
        spriteBatch.DrawRectangle(Bounds, currentBorderColor);

        var currentY = Bounds.Y + MENU_PADDING;

        // Draw title
        if (showTitle)
        {
            spriteBatch.DrawString(menuFont, title, new Vector2(Bounds.X + MENU_PADDING, currentY), currentTextColor);
            currentY += titleHeight;
                
            // Draw title separator
            var separatorRect = new Rectangle(Bounds.X + MENU_PADDING, currentY, Bounds.Width - MENU_PADDING * 2, 1);
            spriteBatch.FillRectangle(separatorRect, currentBorderColor);
            currentY += MENU_PADDING;
        }

        // Draw menu items
        for (var i = 0; i < menuItems.Count; i++)
        {
            var item = menuItems[i];
            var itemRect = new Rectangle(Bounds.X + MENU_PADDING, currentY, 
                menuWidth - MENU_PADDING * 2, MENU_ITEM_HEIGHT);

            // Draw selection background
            if (i == selectedIndex && item.Enabled)
            {
                spriteBatch.FillRectangle(itemRect, currentSelectedColor);
            }

            // Draw text
            var itemTextColor = item.Enabled ? currentTextColor : System.Drawing.Color.Gray;
            spriteBatch.DrawString(menuFont, item.Text, new(itemRect.X, itemRect.Y), itemTextColor);

            currentY += MENU_ITEM_HEIGHT;
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (!isVisible) return false;

        if (e is MouseEvent mouseEvent && Bounds.Contains(mouseEvent.X, mouseEvent.Y))
        {
            switch (mouseEvent.Type)
            {
            case EventType.MouseMove:
                UpdateSelection(new Point(mouseEvent.X, mouseEvent.Y));
                return true;

            case EventType.MouseDown:
                if (mouseEvent.Button == MouseButton.Left && selectedIndex >= 0 && selectedIndex < menuItems.Count)
                {
                    var item = menuItems[selectedIndex];
                    if (item.Enabled)
                    {
                        MenuItemSelected?.Invoke(selectedIndex);
                        if (item.Action != null)
                        {
                            item.Action();
                        }
                        Hide();
                        return true;
                    }
                }
                else if(mouseEvent.Button == MouseButton.Right)
                {
                    Hide();
                    return true;
                }
                return true;
            }
        }
        else if (e is KeyEvent keyEvent)
        {
            switch (keyEvent.Type)
            {
            case EventType.KeyDown:
                if (keyEvent.Key == Silk.NET.Input.Key.Escape)
                {
                    Hide();
                    return true;
                }
                else if (keyEvent.Key == Silk.NET.Input.Key.Up)
                {
                    SelectPrevious();
                    return true;
                }
                else if (keyEvent.Key == Silk.NET.Input.Key.Down)
                {
                    SelectNext();
                    return true;
                }
                else if (keyEvent.Key == Silk.NET.Input.Key.Enter)
                {
                    if (selectedIndex >= 0 && selectedIndex < menuItems.Count)
                    {
                        var item = menuItems[selectedIndex];
                        if (item.Enabled)
                        {
                            MenuItemSelected?.Invoke(selectedIndex);
                            if (item.Action != null)
                            {
                                item.Action();
                            }
                            Hide();
                            return true;
                        }
                    }
                }
                break;
            }
        }

        return base.HandleEvent(e);
    }

    private void UpdateSelection(Point mousePos)
    {
        if (!showTitle)
        {
            var itemIndex = (mousePos.Y - Bounds.Y - MENU_PADDING) / MENU_ITEM_HEIGHT;
            if (itemIndex >= 0 && itemIndex < menuItems.Count)
            {
                selectedIndex = itemIndex;
            }
            else
            {
                selectedIndex = -1;
            }
        }
        else
        {
            var itemIndex = (mousePos.Y - Bounds.Y - titleHeight - MENU_PADDING * 2) / MENU_ITEM_HEIGHT;
            if (itemIndex >= 0 && itemIndex < menuItems.Count)
            {
                selectedIndex = itemIndex;
            }
            else
            {
                selectedIndex = -1;
            }
        }
    }

    private void SelectNext()
    {
        if (menuItems.Count == 0)
            return;

        if (selectedIndex < menuItems.Count - 1)
        {
            selectedIndex++;
        }
        else
        {
            selectedIndex = 0;
        }
    }

    private void SelectPrevious()
    {
        if (menuItems.Count == 0)
            return;

        if (selectedIndex > 0)
        {
            selectedIndex--;
        }
        else
        {
            selectedIndex = menuItems.Count - 1;
        }
    }

    public new bool IsVisible()
    {
        return isVisible;
    }

    public void SetModal(bool modal)
    {
        isModal = modal;
    }

    public new bool IsModal()
    {
        return isModal;
    }

    public void SetColors(System.Drawing.Color background, System.Drawing.Color text, System.Drawing.Color selected, System.Drawing.Color border)
    {
        backgroundColor = background;
        textColor = text;
        selectedColor = selected;
        borderColor = border;
    }

    public void SetFadeSpeed(float speed)
    {
        fadeSpeed = speed;
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    public MenuItem? GetSelectedItem()
    {
        if (selectedIndex >= 0 && selectedIndex < menuItems.Count)
        {
            return menuItems[selectedIndex];
        }
        return null;
    }

    public int GetMenuItemCount()
    {
        return menuItems.Count;
    }

    public MenuItem? GetMenuItem(int index)
    {
        if (index >= 0 && index < menuItems.Count)
        {
            return menuItems[index];
        }
        return null;
    }

    public override void Dispose()
    {
        //menuFont?.Dispose();
        menuItems?.Clear();
        base.Dispose();
    }
}
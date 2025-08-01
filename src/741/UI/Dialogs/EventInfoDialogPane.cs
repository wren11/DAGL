using System;
using System.Collections.Generic;
using System.Text;
using DarkAges.Library.Graphics;
using DarkAges.Library.GameLogic;
using System.Drawing;
using Size = DarkAges.Library.Graphics.Size;

namespace DarkAges.Library.UI.Dialogs;

/// <summary>
/// Dialog pane for displaying event information and details
/// </summary>
public class EventInfoDialogPane : DialogPane, IDisposable
{
    private const int MAX_EVENT_NAME_LENGTH = 256;
    private const int MAX_EVENT_DESC_LENGTH = 1024;
    private const int MAX_REWARD_COUNT = 32;
    private const int ICON_SIZE = 32;

    // Event data
    private EventInfo? eventInfo;
    private bool isDisposed;
    private bool isVisible;
    private bool isEnabled;

    // UI elements
    private ImagePane iconPane = null!;
    private TextPane namePane = null!;
    private TextPane levelPane = null!;
    private TextPane descriptionPane = null!;
    private TextPane requirementsPane = null!;
    private TextPane rewardsPane = null!;
    private ButtonControlPane acceptButton = null!;
    private ButtonControlPane cancelButton = null!;
    private ButtonControlPane closeButton = null!;

    // Layout information
    private Rectangle iconBounds;
    private Rectangle nameBounds;
    private Rectangle levelBounds;
    private Rectangle descriptionBounds;
    private Rectangle requirementsBounds;
    private Rectangle rewardsBounds;
    private Rectangle buttonBounds;

    // Event data storage
    private string eventName = string.Empty;
    private string eventDescription = string.Empty;
    private int eventLevel;
    private int eventIcon;
    private List<string> requirements = new List<string>();
    private List<EventReward> rewards = new List<EventReward>();
    private EventStatus eventStatus;

    // Events
    public event Action<EventInfo> EventAccepted = delegate { };
    public event Action<EventInfo> EventDeclined = delegate { };
    public event Action<EventInfo> EventClosed = delegate { };
    public event Action<EventInfo, EventError> EventError = delegate { };

    public EventInfoDialogPane() : this(null)
    {
    }

    public EventInfoDialogPane(EventInfo? info)
    {
        InitializeDialog();
        if (info != null)
        {
            SetEventInfo(info);
        }
    }

    private void InitializeDialog()
    {
        isDisposed = false;
        isVisible = true;
        isEnabled = true;

        // Initialize data
        eventInfo = null;
        eventName = "";
        eventDescription = "";
        eventLevel = 0;
        eventIcon = 0;
        requirements = [];
        rewards = [];
        eventStatus = EventStatus.Unknown;

        // Initialize UI elements
        InitializeUIElements();
        SetupLayout();
    }

    private void InitializeUIElements()
    {
        // Initialize icon pane
        iconPane = new ImagePane();
        iconPane.Size = new Size(ICON_SIZE, ICON_SIZE);
        AddChild(iconPane);

        // Initialize text panes with SimpleFont
        var defaultFont = FontManager.GetFont("default") as SimpleFont;
        if (defaultFont == null)
        {
            defaultFont = new SimpleFont("Arial", 12);
        }

        namePane = new TextPane("", new Rectangle(), defaultFont);
        namePane.TextColor = System.Drawing.Color.White;
        AddChild(namePane);

        levelPane = new TextPane("", new Rectangle(), defaultFont);
        levelPane.TextColor = System.Drawing.Color.Yellow;
        AddChild(levelPane);

        descriptionPane = new TextPane("", new Rectangle(), defaultFont);
        descriptionPane.TextColor = System.Drawing.Color.White;
        AddChild(descriptionPane);

        requirementsPane = new TextPane("", new Rectangle(), defaultFont);
        requirementsPane.TextColor = System.Drawing.Color.LightBlue;
        AddChild(requirementsPane);

        rewardsPane = new TextPane("", new Rectangle(), defaultFont);
        rewardsPane.TextColor = System.Drawing.Color.LightGreen;
        AddChild(rewardsPane);

        // Create buttons
        acceptButton = new ButtonControlPane();
        acceptButton.SetText("Accept");
        acceptButton.Size = new Size(80, 25);
        acceptButton.Click += OnAcceptClicked;
        AddChild(acceptButton);

        cancelButton = new ButtonControlPane();
        cancelButton.SetText("Cancel");
        cancelButton.Size = new Size(80, 25);
        cancelButton.Click += OnCancelClicked;
        AddChild(cancelButton);

        closeButton = new ButtonControlPane();
        closeButton.SetText("Close");
        closeButton.Size = new Size(80, 25);
        closeButton.Click += OnCloseClicked;
        AddChild(closeButton);
    }

    private void SetupLayout()
    {
        // Set default bounds
        iconBounds = new Rectangle(20, 20, ICON_SIZE, ICON_SIZE);
        nameBounds = new Rectangle(70, 20, 300, 20);
        levelBounds = new Rectangle(70, 45, 100, 15);
        descriptionBounds = new Rectangle(20, 80, 360, 100);
        requirementsBounds = new Rectangle(20, 200, 360, 80);
        rewardsBounds = new Rectangle(20, 300, 360, 120);
        buttonBounds = new Rectangle(20, 440, 360, 30);

        // Apply bounds
        iconPane.Bounds = iconBounds;
        namePane.Bounds = nameBounds;
        levelPane.Bounds = levelBounds;
        descriptionPane.Bounds = descriptionBounds;
        requirementsPane.Bounds = requirementsBounds;
        rewardsPane.Bounds = rewardsBounds;

        // Position buttons
        var buttonWidth = 80;
        var buttonSpacing = 20;
        var startX = buttonBounds.X + (buttonBounds.Width - (buttonWidth * 3 + buttonSpacing * 2)) / 2;

        acceptButton.Bounds = new Rectangle(startX, buttonBounds.Y, buttonWidth, buttonBounds.Height);
        cancelButton.Bounds = new Rectangle(startX + buttonWidth + buttonSpacing, buttonBounds.Y, buttonWidth, buttonBounds.Height);
        closeButton.Bounds = new Rectangle(startX + (buttonWidth + buttonSpacing) * 2, buttonBounds.Y, buttonWidth, buttonBounds.Height);
    }

    public void SetEventInfo(EventInfo info)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(EventInfoDialogPane));

        if (info == null)
            throw new ArgumentNullException(nameof(info));

        try
        {
            eventInfo = info;
            eventName = info.Name ?? "";
            eventDescription = info.Description ?? "";
            eventLevel = info.Level;
            eventIcon = info.IconId;
            eventStatus = info.Status;

            // Parse requirements
            requirements.Clear();
            if (!string.IsNullOrEmpty(info.Requirements))
            {
                string[] reqs = info.Requirements.Split(';');
                foreach (var req in reqs)
                {
                    if (!string.IsNullOrWhiteSpace(req))
                    {
                        requirements.Add(req.Trim());
                    }
                }
            }

            // Parse rewards
            rewards.Clear();
            if (info.Rewards != null)
            {
                foreach (var reward in info.Rewards)
                {
                    rewards.Add(reward);
                }
            }

            UpdateDisplay();
        }
        catch (Exception ex)
        {
            EventError?.Invoke(info, new EventError
            {
                ErrorCode = EventErrorCode.InvalidEventData,
                Message = $"Failed to set event info: {ex.Message}",
                Exception = ex
            });
        }
    }

    public EventInfo? GetEventInfo()
    {
        return eventInfo;
    }

    private void UpdateDisplay()
    {
        if (isDisposed || eventInfo == null)
            return;

        try
        {
            // Update icon
            if (eventIcon > 0)
            {
                LoadEventIcon(eventIcon);
            }

            // Update text
            namePane.Text = eventName;
            levelPane.Text = $"Level: {eventLevel}";

            // Update description
            descriptionPane.Text = eventDescription;

            // Update requirements
            var reqText = "Requirements:";
            if (requirements.Count > 0)
            {
                reqText += "\n" + string.Join("\n", requirements);
            }
            else
            {
                reqText += "\nNone";
            }
            requirementsPane.Text = reqText;

            // Update rewards
            var rewardText = "Rewards:";
            if (rewards.Count > 0)
            {
                foreach (var reward in rewards)
                {
                    rewardText += $"\n• {reward.Name} x{reward.Quantity}";
                }
            }
            else
            {
                rewardText += "\nNone";
            }
            rewardsPane.Text = rewardText;

            // Update button visibility based on event status
            UpdateButtonVisibility();
        }
        catch (Exception ex)
        {
            EventError?.Invoke(eventInfo, new EventError
            {
                ErrorCode = EventErrorCode.DisplayUpdateFailed,
                Message = $"Failed to update display: {ex.Message}",
                Exception = ex
            });
        }
    }

    private void LoadEventIcon(int iconId)
    {
        try
        {
            // Try to load from EPF file first
            var iconPath = "leicon.epf";
            var defaultPalette = new Palette();
                
            if (System.IO.File.Exists(iconPath))
            {
                // Try to load actual icon from EPF
                var iconImage = ImageManager.LoadImage(iconPath, iconId);
                if (iconImage != null)
                {
                    iconPane.SetImage(iconImage, defaultPalette);
                    return;
                }
            }
                
            // Create a procedural event icon based on iconId
            var placeholderIcon = CreateEventIcon(iconId);
            iconPane.SetImage(placeholderIcon, defaultPalette);
        }
        catch
        {
            // Fallback to a default event icon
            LoadEventIcon_Fallback();
        }
    }

    private void LoadEventIcon_Fallback()
    {
        if (iconPane != null)
        {
            // Set default icon or clear
            iconPane.SetImage(CreateDefaultEventIcon(), new Palette());
        }
    }

    private IndexedImage CreateEventIcon(int iconId)
    {
        var iconImage = new IndexedImage(32, 32);
        var pixels = new ColorRgb555[32 * 32];
            
        // Clear background
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new ColorRgb555(0); // Transparent background
        }
            
        // Draw icon based on type
        switch (iconId)
        {
        case 1: // Quest
            DrawQuestIcon(pixels, 32, 32);
            break;
        case 2: // Combat
            DrawCombatIcon(pixels, 32, 32);
            break;
        case 3: // Trade
            DrawTradeIcon(pixels, 32, 32);
            break;
        case 4: // Social
            DrawSocialIcon(pixels, 32, 32);
            break;
        case 5: // System
            DrawSystemIcon(pixels, 32, 32);
            break;
        default:
            DrawExclamationIcon(pixels, 32, 32);
            break;
        }
            
        // Convert ColorRgb555[] to byte[] for SetPixelData
        var bytePixels = new byte[pixels.Length];
        for (var i = 0; i < pixels.Length; i++)
        {
            // Convert ColorRgb555 to ColorRgb565
            var color555 = pixels[i];
            var color565 = new ColorRgb565(
                (byte)(color555.R << 3),  // Convert 5-bit to 8-bit
                (byte)(color555.G << 3),  // Convert 5-bit to 8-bit  
                (byte)(color555.B << 3)   // Convert 5-bit to 8-bit
            );
            bytePixels[i] = (byte)color565.Value;
        }
            
        iconImage.SetPixelData(bytePixels);
        return iconImage;
    }

    private IndexedImage CreateDefaultEventIcon()
    {
        var iconImage = new IndexedImage(32, 32);
        var pixels = new ColorRgb555[32 * 32];
            
        // Draw a simple exclamation mark
        DrawExclamationIcon(pixels, 32, 32);
            
        // Convert ColorRgb555[] to byte[] for SetPixelData
        var bytePixels = new byte[pixels.Length];
        for (var i = 0; i < pixels.Length; i++)
        {
            // Convert ColorRgb555 to ColorRgb565
            var color555 = pixels[i];
            var color565 = new ColorRgb565(
                (byte)(color555.R << 3),  // Convert 5-bit to 8-bit
                (byte)(color555.G << 3),  // Convert 5-bit to 8-bit  
                (byte)(color555.B << 3)   // Convert 5-bit to 8-bit
            );
            bytePixels[i] = (byte)color565.Value;
        }
            
        iconImage.SetPixelData(bytePixels);
        return iconImage;
    }

    private void DrawQuestIcon(ColorRgb555[] pixels, int width, int height)
    {
        // Draw a scroll icon
        DrawRectangle(pixels, width, height, 8, 6, 16, 20, 1);
        DrawRectangle(pixels, width, height, 6, 8, 4, 2, 1);
        DrawRectangle(pixels, width, height, 22, 8, 4, 2, 1);
    }

    private void DrawCombatIcon(ColorRgb555[] pixels, int width, int height)
    {
        // Draw crossed swords
        DrawLine(pixels, width, height, 8, 8, 24, 24, 1);
        DrawLine(pixels, width, height, 24, 8, 8, 24, 1);
    }

    private void DrawTradeIcon(ColorRgb555[] pixels, int width, int height)
    {
        // Draw coins
        DrawCircle(pixels, width, height, 12, 16, 4, 1);
        DrawCircle(pixels, width, height, 20, 16, 4, 1);
    }

    private void DrawSocialIcon(ColorRgb555[] pixels, int width, int height)
    {
        // Draw people silhouettes
        DrawCircle(pixels, width, height, 10, 12, 3, 1);
        DrawCircle(pixels, width, height, 22, 12, 3, 1);
        DrawRectangle(pixels, width, height, 8, 16, 4, 8, 1);
        DrawRectangle(pixels, width, height, 20, 16, 4, 8, 1);
    }

    private void DrawSystemIcon(ColorRgb555[] pixels, int width, int height)
    {
        // Draw gear icon
        DrawRectangle(pixels, width, height, 14, 14, 4, 4, 1);
        // Add gear teeth
        for (var i = 0; i < 8; i++)
        {
            var angle = i * 45;
            var x = (int)(16 + 8 * Math.Cos(angle * Math.PI / 180));
            var y = (int)(16 + 8 * Math.Sin(angle * Math.PI / 180));
            if (x >= 0 && x < width && y >= 0 && y < height)
                pixels[y * width + x] = new ColorRgb555(1);
        }
    }

    private void DrawExclamationIcon(ColorRgb555[] pixels, int width, int height)
    {
        // Draw exclamation mark
        DrawRectangle(pixels, width, height, 15, 8, 2, 12, 1);
        DrawRectangle(pixels, width, height, 15, 22, 2, 2, 1);
    }

    private void DrawLine(ColorRgb555[] pixels, int width, int height, int x1, int y1, int x2, int y2, ushort color)
    {
        var dx = Math.Abs(x2 - x1);
        var dy = Math.Abs(y2 - y1);
        var sx = x1 < x2 ? 1 : -1;
        var sy = y1 < y2 ? 1 : -1;
        var err = dx - dy;

        while (true)
        {
            if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                pixels[y1 * width + x1] = new ColorRgb555(color);

            if (x1 == x2 && y1 == y2) break;
                
            var e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x1 += sx; }
            if (e2 < dx) { err += dx; y1 += sy; }
        }
    }

    private void DrawCircle(ColorRgb555[] pixels, int width, int height, int centerX, int centerY, int radius, ushort color)
    {
        for (var y = -radius; y <= radius; y++)
        {
            for (var x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    var pixelX = centerX + x;
                    var pixelY = centerY + y;
                    if (pixelX >= 0 && pixelX < width && pixelY >= 0 && pixelY < height)
                    {
                        pixels[pixelY * width + pixelX] = new ColorRgb555(color);
                    }
                }
            }
        }
    }

    private void DrawRectangle(ColorRgb555[] pixels, int width, int height, int x, int y, int rectWidth, int rectHeight, ushort color)
    {
        for (var dy = 0; dy < rectHeight; dy++)
        {
            for (var dx = 0; dx < rectWidth; dx++)
            {
                var pixelX = x + dx;
                var pixelY = y + dy;
                if (pixelX >= 0 && pixelX < width && pixelY >= 0 && pixelY < height)
                {
                    pixels[pixelY * width + pixelX] = new ColorRgb555(color);
                }
            }
        }
    }

    private void UpdateButtonVisibility()
    {
        if (eventInfo == null)
            return;

        switch (eventStatus)
        {
        case EventStatus.Available:
            acceptButton.IsVisible = true;
            cancelButton.IsVisible = true;
            closeButton.IsVisible = false;
            break;
        case EventStatus.InProgress:
            acceptButton.IsVisible = false;
            cancelButton.IsVisible = false;
            closeButton.IsVisible = true;
            break;
        case EventStatus.Completed:
            acceptButton.IsVisible = false;
            cancelButton.IsVisible = false;
            closeButton.IsVisible = true;
            break;
        case EventStatus.Failed:
            acceptButton.IsVisible = false;
            cancelButton.IsVisible = false;
            closeButton.IsVisible = true;
            break;
        default:
            acceptButton.IsVisible = false;
            cancelButton.IsVisible = false;
            closeButton.IsVisible = true;
            break;
        }
    }

    private void OnAcceptClicked(object? sender, EventArgs e)
    {
        if (isDisposed || eventInfo == null)
            return;

        try
        {
            EventAccepted?.Invoke(eventInfo);
            Hide();
        }
        catch (Exception ex)
        {
            EventError?.Invoke(eventInfo, new EventError
            {
                ErrorCode = EventErrorCode.AcceptFailed,
                Message = $"Failed to accept event: {ex.Message}",
                Exception = ex
            });
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        if (isDisposed || eventInfo == null)
            return;

        try
        {
            EventDeclined?.Invoke(eventInfo);
            Hide();
        }
        catch (Exception ex)
        {
            EventError?.Invoke(eventInfo, new EventError
            {
                ErrorCode = EventErrorCode.DeclineFailed,
                Message = $"Failed to decline event: {ex.Message}",
                Exception = ex
            });
        }
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        if (isDisposed || eventInfo == null)
            return;

        try
        {
            EventClosed?.Invoke(eventInfo);
            Hide();
        }
        catch (Exception ex)
        {
            EventError?.Invoke(eventInfo, new EventError
            {
                ErrorCode = EventErrorCode.CloseFailed,
                Message = $"Failed to close event dialog: {ex.Message}",
                Exception = ex
            });
        }
    }

    public void SetVisible(bool visible)
    {
        if (isDisposed)
            return;

        isVisible = visible;
        base.IsVisible = visible;
    }

    public void SetEnabled(bool enabled)
    {
        if (isDisposed)
            return;

        isEnabled = enabled;
        base.IsEnabled = enabled;
    }

    public bool IsVisible()
    {
        return isVisible && !isDisposed;
    }

    public bool IsEnabled()
    {
        return isEnabled && !isDisposed;
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // Dispose UI elements
                iconPane?.Dispose();
                namePane?.Dispose();
                levelPane?.Dispose();
                descriptionPane?.Dispose();
                requirementsPane?.Dispose();
                rewardsPane?.Dispose();
                acceptButton?.Dispose();
                cancelButton?.Dispose();
                closeButton?.Dispose();

                // Clear data
                eventInfo = null;
                requirements?.Clear();
                rewards?.Clear();
            }

            isDisposed = true;
        }
    }
}

internal class ImageManager
{
    public static IndexedImage? LoadImage(string path, int iconId)
    {
        // Placeholder for actual image loading logic
        // This should load an image from the specified path and return it as an IndexedImage
        return new IndexedImage(32, 32); // Return a blank image for now
    }
}
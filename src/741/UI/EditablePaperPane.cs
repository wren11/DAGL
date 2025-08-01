using System;
using System.Drawing;
using System.Text;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using Silk.NET.Input;
using DarkAges.Library.IO;
using TextCopy;
using Size = DarkAges.Library.Graphics.Size;
using Vector2 = DarkAges.Library.Graphics.Vector2;

namespace DarkAges.Library.UI;

/// <summary>
/// Handles editable text input with paper-like appearance
/// </summary>
public class EditablePaperPane : Pane
{
    private const int MAX_TEXT_LENGTH = 8000;
    private const int DEFAULT_WIDTH = 430;
    private const int DEFAULT_HEIGHT = 12;
    private const int TEXT_PADDING = 5;
    private const int CURSOR_BLINK_RATE = 500; // milliseconds

    private StringBuilder textBuffer;
    private int cursorPosition;
    private int selectionStart;
    private int selectionEnd;
    private bool hasSelection;
    private bool cursorVisible;
    private DateTime lastCursorBlink;
    private bool isEditable;
    private bool isMultiline;
    private int maxLength;
    private Color backgroundColor;
    private System.Drawing.Color textColor;
    private System.Drawing.Color selectionColor;
    private System.Drawing.Color cursorColor;
    private SimpleFont textFont;
    private string placeholderText;
    private bool showPlaceholder;

    // Input mode
    private int inputMode; // 0 = normal, 1 = special
        
    public string Text
    {
        get => GetText();
        set => SetText(value);
    }
        
    public int MaxLength
    {
        get => GetMaxLength();
        set => SetMaxLength(value);
    }

    public EditablePaperPane()
    {
        InitializePaper();
        LoadLayout();
    }

    public EditablePaperPane(int width, int height, int mode)
    {
        InitializePaper();
        LoadLayout();
        Size = new Size(width, height);
        inputMode = mode;
    }

    private void InitializePaper()
    {
        textBuffer = new StringBuilder();
        cursorPosition = 0;
        selectionStart = 0;
        selectionEnd = 0;
        hasSelection = false;
        cursorVisible = true;
        lastCursorBlink = DateTime.Now;
        isEditable = true;
        isMultiline = false;
        maxLength = MAX_TEXT_LENGTH;
        placeholderText = "";
        showPlaceholder = false;
        inputMode = 0;
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_editablepaper.txt");
            Bounds = layout.GetRect("Bounds", new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT));
            backgroundColor = layout.GetColor("BackgroundColor", Color.White);
            textColor = layout.GetColor("TextColor", Color.Black);
            selectionColor = layout.GetColor("SelectionColor", Color.FromArgb(255, 0, 120, 215));
            cursorColor = layout.GetColor("CursorColor", Color.Black);
            var fontName = layout.GetString("FontName", "Arial");
            var fontSize = layout.GetInt("FontSize", 10);
            textFont = new SimpleFont(fontName, fontSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading editable paper layout: {ex.Message}");
            // Fallback to defaults
            Bounds = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            backgroundColor = Color.White;
            textColor = Color.Black;
            selectionColor = Color.FromArgb(255, 0, 120, 215);
            cursorColor = Color.Black;
            textFont = new SimpleFont("Arial", 10);
        }
    }

    public void SetText(string text)
    {
        if (text == null)
            text = "";

        if (text.Length > maxLength)
            text = text.Substring(0, maxLength);

        textBuffer.Clear();
        textBuffer.Append(text);
        cursorPosition = textBuffer.Length;
        ClearSelection();
    }

    public string GetText()
    {
        return textBuffer.ToString();
    }

    public void Clear()
    {
        textBuffer.Clear();
        cursorPosition = 0;
        ClearSelection();
    }

    public void SetPlaceholder(string placeholder)
    {
        placeholderText = placeholder ?? "";
        showPlaceholder = !string.IsNullOrEmpty(placeholderText);
    }

    public void SetEditable(bool editable)
    {
        isEditable = editable;
    }

    public bool IsEditable()
    {
        return isEditable;
    }

    public void SetMultiline(bool multiline)
    {
        isMultiline = multiline;
    }

    public bool IsMultiline()
    {
        return isMultiline;
    }

    public void SetMaxLength(int maxLen)
    {
        maxLength = Math.Max(0, Math.Min(maxLen, MAX_TEXT_LENGTH));
        if (textBuffer.Length > maxLength)
        {
            textBuffer.Length = maxLength;
            if (cursorPosition > maxLength)
                cursorPosition = maxLength;
        }
    }

    public int GetMaxLength()
    {
        return maxLength;
    }

    public void SetCursorPosition(int position)
    {
        cursorPosition = Math.Max(0, Math.Min(position, textBuffer.Length));
        ClearSelection();
    }

    public int GetCursorPosition()
    {
        return cursorPosition;
    }

    public void SelectAll()
    {
        selectionStart = 0;
        selectionEnd = textBuffer.Length;
        hasSelection = true;
        cursorPosition = textBuffer.Length;
    }

    public void ClearSelection()
    {
        hasSelection = false;
        selectionStart = 0;
        selectionEnd = 0;
    }

    public string GetSelectedText()
    {
        if (!hasSelection)
            return "";

        var start = Math.Min(selectionStart, selectionEnd);
        var end = Math.Max(selectionStart, selectionEnd);
        return textBuffer.ToString(start, end - start);
    }

    public void DeleteSelection()
    {
        if (!hasSelection)
            return;

        var start = Math.Min(selectionStart, selectionEnd);
        var end = Math.Max(selectionStart, selectionEnd);
            
        textBuffer.Remove(start, end - start);
        cursorPosition = start;
        ClearSelection();
    }

    public void InsertText(string text)
    {
        if (!isEditable || text == null)
            return;

        DeleteSelection();

        if (textBuffer.Length + text.Length > maxLength)
        {
            var remainingSpace = maxLength - textBuffer.Length;
            if (remainingSpace > 0)
            {
                text = text.Substring(0, remainingSpace);
            }
            else
            {
                return;
            }
        }

        textBuffer.Insert(cursorPosition, text);
        cursorPosition += text.Length;
    }

    public void SetColors(System.Drawing.Color background, System.Drawing.Color text, System.Drawing.Color selection, System.Drawing.Color cursor)
    {
        backgroundColor = background;
        textColor = text;
        selectionColor = selection;
        cursorColor = cursor;
    }

    public void SetFont(SimpleFont font)
    {
        textFont = font;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Update cursor blink
        if (DateTime.Now - lastCursorBlink > TimeSpan.FromMilliseconds(CURSOR_BLINK_RATE))
        {
            cursorVisible = !cursorVisible;
            lastCursorBlink = DateTime.Now;
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        // Draw background
        spriteBatch.FillRectangle(Bounds, backgroundColor);

        // Draw border
        spriteBatch.DrawRectangle(Bounds, System.Drawing.Color.Gray);

        // Determine text to display
        var displayText = textBuffer.ToString();
        if (string.IsNullOrEmpty(displayText) && showPlaceholder)
        {
            displayText = placeholderText;
            spriteBatch.DrawString(textFont, displayText, new Vector2(Bounds.X + TEXT_PADDING, Bounds.Y + TEXT_PADDING), System.Drawing.Color.Gray);
            return;
        }

        if (string.IsNullOrEmpty(displayText))
            return;

        // Draw selection background
        if (hasSelection)
        {
            DrawSelection(spriteBatch, displayText);
        }

        // Draw text
        spriteBatch.DrawString(textFont, displayText, new Vector2(Bounds.X + TEXT_PADDING, Bounds.Y + TEXT_PADDING), textColor);

        // Draw cursor
        if (cursorVisible && isEditable)
        {
            DrawCursor(spriteBatch, displayText);
        }
    }

    private void DrawSelection(SpriteBatch spriteBatch, string text)
    {
        if (!hasSelection)
            return;

        var start = Math.Min(selectionStart, selectionEnd);
        var end = Math.Max(selectionStart, selectionEnd);

        if (start >= text.Length)
            return;

        end = Math.Min(end, text.Length);

        var beforeSelection = text.Substring(0, start);
        var selectedText = text.Substring(start, end - start);

        var beforeSize = textFont.MeasureString(beforeSelection);
        var selectedSize = textFont.MeasureString(selectedText);

        var selectionRect = new Rectangle(
            Bounds.X + TEXT_PADDING + beforeSize.Width,
            Bounds.Y + TEXT_PADDING,
            selectedSize.Width,
            selectedSize.Height
        );
            
        spriteBatch.FillRectangle(selectionRect, selectionColor);
    }

    private void DrawCursor(SpriteBatch spriteBatch, string text)
    {
        var beforeCursor = text.Substring(0, cursorPosition);
        var beforeSize = textFont.MeasureString(beforeCursor);

        var cursorX = Bounds.X + TEXT_PADDING + beforeSize.Width;
        var cursorY = Bounds.Y + TEXT_PADDING;
        var cursorHeight = textFont.MeasureString("A").Height;

        spriteBatch.DrawRectangle(new Rectangle(cursorX, cursorY, 1, cursorHeight), cursorColor);
    }

    public override bool HandleEvent(Event e)
    {
        if (e is MouseEvent mouseEvent && Bounds.Contains(mouseEvent.X, mouseEvent.Y))
        {
            switch (mouseEvent.Type)
            {
            case EventType.MouseDown:
                SetCursorFromMousePosition(new Point(mouseEvent.X, mouseEvent.Y));
                ClearSelection();
                return true;

            case EventType.MouseMove:
                UpdateSelectionFromMousePosition(new Point(mouseEvent.X, mouseEvent.Y));
                return true;
            }
        }
        else if (e is KeyEvent keyEvent)
        {
            if (!isEditable)
                return false;

            switch (keyEvent.Type)
            {
            case EventType.KeyDown:
                return HandleKeyDown(keyEvent.Key, (KeyModifiers)keyEvent.Modifiers);

            case EventType.KeyUp:
                if (keyEvent.Key >= Key.A && keyEvent.Key <= Key.Z)
                {
                    InsertText(((char)keyEvent.Key).ToString());
                    return true;
                }
                break;
            }
        }

        return base.HandleEvent(e);
    }

    private bool HandleKeyDown(Key key, KeyModifiers modifiers)
    {
        switch (key)
        {
        case Silk.NET.Input.Key.Left:
            if (modifiers.HasFlag(KeyModifiers.Shift))
            {
                StartSelection();
                MoveCursorLeft();
                UpdateSelection();
            }
            else
            {
                ClearSelection();
                MoveCursorLeft();
            }
            return true;

        case Silk.NET.Input.Key.Right:
            if (modifiers.HasFlag(KeyModifiers.Shift))
            {
                StartSelection();
                MoveCursorRight();
                UpdateSelection();
            }
            else
            {
                ClearSelection();
                MoveCursorRight();
            }
            return true;

        case Silk.NET.Input.Key.Home:
            if (modifiers.HasFlag(KeyModifiers.Shift))
            {
                StartSelection();
                cursorPosition = 0;
                UpdateSelection();
            }
            else
            {
                ClearSelection();
                cursorPosition = 0;
            }
            return true;

        case Silk.NET.Input.Key.End:
            if (modifiers.HasFlag(KeyModifiers.Shift))
            {
                StartSelection();
                cursorPosition = textBuffer.Length;
                UpdateSelection();
            }
            else
            {
                ClearSelection();
                cursorPosition = textBuffer.Length;
            }
            return true;

        case Silk.NET.Input.Key.Backspace:
            if (hasSelection)
            {
                DeleteSelection();
            }
            else if (cursorPosition > 0)
            {
                textBuffer.Remove(cursorPosition - 1, 1);
                cursorPosition--;
            }
            return true;

        case Silk.NET.Input.Key.Delete:
            if (hasSelection)
            {
                DeleteSelection();
            }
            else if (cursorPosition < textBuffer.Length)
            {
                textBuffer.Remove(cursorPosition, 1);
            }
            return true;

        case Silk.NET.Input.Key.Enter:
            if (isMultiline)
            {
                InsertText("\n");
            }
            return true;

        case Silk.NET.Input.Key.A:
            if (modifiers.HasFlag(KeyModifiers.Control))
            {
                SelectAll();
                return true;
            }
            break;

        case Silk.NET.Input.Key.C:
            if (modifiers.HasFlag(KeyModifiers.Control))
            {
                // Copy selected text to clipboard
                var selectedText = GetSelectedText();
                if (!string.IsNullOrEmpty(selectedText))
                {
                    // Cross-platform clipboard implementation
                    Console.WriteLine($"Copy to clipboard requested: {selectedText}");
                    SetTextToClipboard(selectedText);
                }
                return true;
            }
            break;

        case Silk.NET.Input.Key.V:
            if (modifiers.HasFlag(KeyModifiers.Control))
            {
                // Paste from clipboard
                Console.WriteLine("Paste from clipboard requested");
                var clipboardText = GetTextFromClipboard();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    InsertText(clipboardText);
                }
                return true;
            }
            break;

        case Silk.NET.Input.Key.X:
            if (modifiers.HasFlag(KeyModifiers.Control))
            {
                // Cut selected text
                var cutText = GetSelectedText();
                if (!string.IsNullOrEmpty(cutText))
                {
                    // Cross-platform clipboard implementation
                    Console.WriteLine($"Cut to clipboard requested: {cutText}");
                    SetTextToClipboard(cutText);
                    DeleteSelection();
                }
                return true;
            }
            break;
        }

        return false;
    }

    private void MoveCursorLeft()
    {
        if (cursorPosition > 0)
            cursorPosition--;
    }

    private void MoveCursorRight()
    {
        if (cursorPosition < textBuffer.Length)
            cursorPosition++;
    }

    private void StartSelection()
    {
        if (!hasSelection)
        {
            selectionStart = cursorPosition;
            hasSelection = true;
        }
    }

    private void UpdateSelection()
    {
        selectionEnd = cursorPosition;
    }

    private void SetCursorFromMousePosition(Point mousePos)
    {
        if (textFont == null) return;

        var font = textFont as SimpleFont;
        if (font == null) return;

        var text = showPlaceholder ? placeholderText : textBuffer.ToString();
        var textWidth = font.MeasureString(text).Width;
        var relativeX = mousePos.X - (Bounds.X + TEXT_PADDING);

        if (relativeX <= 0)
        {
            cursorPosition = 0;
        }
        else if (relativeX >= textWidth)
        {
            cursorPosition = text.Length;
        }
        else
        {
            // Find the closest character position
            var currentWidth = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var charWidth = font.MeasureString(text[i].ToString()).Width;
                if (currentWidth + charWidth / 2 > relativeX)
                {
                    cursorPosition = i;
                    break;
                }
                currentWidth += charWidth;
                cursorPosition = i + 1;
            }
        }
    }

    private void UpdateSelectionFromMousePosition(Point mousePos)
    {
        if (!hasSelection)
        {
            StartSelection();
        }

        SetCursorFromMousePosition(mousePos);
        UpdateSelection();
    }

    public int GetTextLength()
    {
        return textBuffer.Length;
    }

    public bool HasSelection()
    {
        return hasSelection;
    }

    public int GetSelectionStart()
    {
        return selectionStart;
    }

    public int GetSelectionEnd()
    {
        return selectionEnd;
    }

    private void SetTextToClipboard(string text)
    {
        try
        {
            ClipboardService.SetText(text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to set text to clipboard: {ex.Message}");
        }
    }

    private string GetTextFromClipboard()
    {
        try
        {
            return ClipboardService.GetText() ?? "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get text from clipboard: {ex.Message}");
        }
        return "";
    }

    public override void Dispose()
    {
        textFont?.Dispose();
        base.Dispose();
    }
}
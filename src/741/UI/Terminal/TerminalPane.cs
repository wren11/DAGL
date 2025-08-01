using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DarkAges.Library.Core;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Terminal;

/// <summary>
/// Handles terminal/console functionality with text display and input processing
/// </summary>
public class TerminalPane : ControlPane, IDisposable
{
    private const int MAX_LINES = 1000;
    private const int MAX_LINE_LENGTH = 256;
    private const int SCROLLBACK_SIZE = 100;
    private const int DEFAULT_WIDTH = 80;
    private const int DEFAULT_HEIGHT = 25;

    private List<string> textLines;
    private StringBuilder currentLine;
    private int cursorX;
    private int cursorY;
    private int scrollPosition;
    private bool isVisible;
    private bool isEnabled;
    private bool isDisposed;
    private int terminalMode;
    private bool isConnected;
    private bool isInputMode;
    private string prompt;
    private string lastCommand;
    private DateTime lastActivity;
    private int terminalState;

    // Terminal colors and formatting
    private ConsoleColor foregroundColor;
    private ConsoleColor backgroundColor;
    private bool isBold;
    private bool isUnderline;
    private bool isReverse;

    // Input handling
    private StringBuilder inputBuffer;
    private int inputPosition;
    private List<string> commandHistory;
    private int historyPosition;

    // Events
    public event Action<string>? CommandEntered;
    public event Action<string>? TextReceived;
    public event Action<string>? TextSent;
    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<TerminalError>? TerminalError;

    public TerminalPane() : this(DEFAULT_WIDTH, DEFAULT_HEIGHT)
    {
    }

    public TerminalPane(int width, int height)
    {
        InitializeTerminal(width, height);
    }

    private void InitializeTerminal(int width, int height)
    {
        textLines = [];
        currentLine = new StringBuilder();
        cursorX = 0;
        cursorY = 0;
        scrollPosition = 0;
        isVisible = true;
        isEnabled = true;
        isDisposed = false;
        terminalMode = 0;
        isConnected = false;
        isInputMode = false;
        prompt = "> ";
        lastCommand = "";
        lastActivity = DateTime.Now;

        // Initialize colors
        foregroundColor = ConsoleColor.White;
        backgroundColor = ConsoleColor.Black;
        isBold = false;
        isUnderline = false;
        isReverse = false;

        // Initialize input handling
        inputBuffer = new StringBuilder();
        inputPosition = 0;
        commandHistory = [];
        historyPosition = -1;

        // Add initial line
        AddLine("");
    }

    public void Write(string text)
    {
        if (isDisposed || !isEnabled)
            return;

        try
        {
            ProcessText(text);
            lastActivity = DateTime.Now;
            TextReceived?.Invoke(text);
        }
        catch (Exception ex)
        {
            TerminalError?.Invoke(new TerminalError
            {
                ErrorCode = TerminalErrorCode.WriteFailed,
                Message = $"Failed to write text: {ex.Message}",
                Exception = ex
            });
        }
    }

    public void WriteLine(string text)
    {
        Write(text + "\n");
    }

    public void WriteLine()
    {
        Write("\n");
    }

    private void ProcessText(string text)
    {
        foreach (var c in text)
        {
            ProcessCharacter(c);
        }
    }

    private void ProcessCharacter(char c)
    {
        switch (terminalState)
        {
        case 0: // Normal state
            ProcessNormalCharacter(c);
            break;
        case 1: // Escape sequence
            ProcessEscapeSequence(c);
            break;
        case 4: // Parameter sequence
            ProcessParameterSequence(c);
            break;
        case 5: // Special sequence
            ProcessSpecialSequence(c);
            break;
        case 6: // Negotiation sequence
            ProcessNegotiationSequence(c);
            break;
        case 7: // Connected state
            ProcessConnectedCharacter(c);
            break;
        default:
            terminalState = 0;
            ProcessNormalCharacter(c);
            break;
        }
    }

    private void ProcessNormalCharacter(char c)
    {
        switch (c)
        {
        case '\0': // Null
            break;
        case '\a': // Bell
            PlayBell();
            break;
        case '\b': // Backspace
            HandleBackspace();
            break;
        case '\t': // Tab
            HandleTab();
            break;
        case '\n': // Line feed
            HandleLineFeed();
            break;
        case '\r': // Carriage return
            HandleCarriageReturn();
            break;
        case '\x1B': // Escape
            terminalState = 1;
            break;
        case '\xFF': // Special terminal command
            terminalState = 5;
            break;
        default:
            if (c >= 32 && c <= 126) // Printable characters
            {
                AddCharacter(c);
            }
            break;
        }
    }

    private void ProcessEscapeSequence(char c)
    {
        switch (c)
        {
        case '[':
            terminalState = 4; // Parameter sequence
            break;
        case '(':
        case ')':
        case '*':
        case '+':
            terminalState = 4; // Parameter sequence
            break;
        case 'S':
            HandleServerMode();
            terminalState = 7;
            break;
        case 'C':
            HandleClientMode();
            terminalState = 7;
            break;
        default:
            terminalState = 0;
            break;
        }
    }

    private void ProcessParameterSequence(char c)
    {
        if (c >= '0' && c <= '9')
        {
            // Collect parameter
            terminalState = 4;
        }
        else if (c >= 'A' && c <= 'Z')
        {
            // Process command
            ProcessTerminalCommand(c);
            terminalState = 0;
        }
        else
        {
            terminalState = 0;
        }
    }

    private void ProcessSpecialSequence(char c)
    {
        if (c == '\xFD') // 253
        {
            terminalState = 6; // Negotiation sequence
        }
        else
        {
            terminalState = 0;
        }
    }

    private void ProcessNegotiationSequence(char c)
    {
        switch (c)
        {
        case '\x18': // 24 - Terminal type
            HandleTerminalType();
            break;
        default:
            HandleTerminalOption(c);
            break;
        }
        terminalState = 0;
    }

    private void ProcessConnectedCharacter(char c)
    {
        // Handle connected mode characters
        if (c == '\x1B')
        {
            terminalState = 0;
        }
        else
        {
            ProcessNormalCharacter(c);
        }
    }

    private void ProcessTerminalCommand(char command)
    {
        switch (command)
        {
        case 'A': // Cursor up
            MoveCursorUp();
            break;
        case 'B': // Cursor down
            MoveCursorDown();
            break;
        case 'C': // Cursor right
            MoveCursorRight();
            break;
        case 'D': // Cursor left
            MoveCursorLeft();
            break;
        case 'H': // Cursor home
            MoveCursorHome();
            break;
        case 'J': // Clear screen
            ClearScreen();
            break;
        case 'K': // Clear line
            ClearLine();
            break;
        case 'm': // Set attributes
            SetAttributes();
            break;
        default:
            break;
        }
    }

    private void HandleServerMode()
    {
        isConnected = true;
        WriteLine("Server Connected Mode");
        Connected?.Invoke();
    }

    private void HandleClientMode()
    {
        isConnected = true;
        WriteLine("Connected Server Mode");
        Connected?.Invoke();
    }

    private void HandleTerminalType()
    {
        var terminalType = "dumb";
        SendTerminalResponse($"\xFF\xFB\x18\xFF\xFA\x18{terminalType}\xFF\xF0");
    }

    private void HandleTerminalOption(char option)
    {
        SendTerminalResponse($"\xFF\xFC{option}");
    }

    private void SendTerminalResponse(string response)
    {
        // Send response to server
        TextSent?.Invoke(response);
    }

    private void AddCharacter(char c)
    {
        if (cursorX < MAX_LINE_LENGTH - 1)
        {
            if (cursorX >= currentLine.Length)
            {
                currentLine.Append(c);
            }
            else
            {
                currentLine[cursorX] = c;
            }
            cursorX++;
        }
    }

    private void HandleBackspace()
    {
        if (cursorX > 0)
        {
            cursorX--;
            if (cursorX < currentLine.Length)
            {
                currentLine.Remove(cursorX, 1);
            }
        }
    }

    private void HandleTab()
    {
        // Tab to next 8-character boundary
        var tabStop = ((cursorX / 8) + 1) * 8;
        while (cursorX < tabStop && cursorX < MAX_LINE_LENGTH - 1)
        {
            AddCharacter(' ');
        }
    }

    private void HandleLineFeed()
    {
        cursorY++;
        if (cursorY >= GetHeight())
        {
            ScrollUp();
            cursorY = GetHeight() - 1;
        }
    }

    private void HandleCarriageReturn()
    {
        cursorX = 0;
    }

    private void MoveCursorUp()
    {
        if (cursorY > 0)
            cursorY--;
    }

    private void MoveCursorDown()
    {
        if (cursorY < GetHeight() - 1)
            cursorY++;
    }

    private void MoveCursorRight()
    {
        if (cursorX < MAX_LINE_LENGTH - 1)
            cursorX++;
    }

    private void MoveCursorLeft()
    {
        if (cursorX > 0)
            cursorX--;
    }

    private void MoveCursorHome()
    {
        cursorX = 0;
        cursorY = 0;
    }

    private void ClearScreen()
    {
        textLines.Clear();
        AddLine("");
        cursorX = 0;
        cursorY = 0;
    }

    private void ClearLine()
    {
        if (cursorY < textLines.Count)
        {
            textLines[cursorY] = "";
        }
        cursorX = 0;
    }

    private void SetAttributes()
    {
        // Handle color and attribute codes
        // Implementation depends on specific terminal codes
    }

    private void PlayBell()
    {
        // Play terminal bell sound
        Console.Beep();
    }

    private void ScrollUp()
    {
        if (textLines.Count > 0)
        {
            textLines.RemoveAt(0);
            AddLine("");
        }
    }

    private void AddLine(string text)
    {
        textLines.Add(text);
        if (textLines.Count > MAX_LINES)
        {
            textLines.RemoveAt(0);
        }
    }

    public void SetPrompt(string newPrompt)
    {
        prompt = newPrompt ?? "> ";
    }

    public string GetPrompt()
    {
        return prompt;
    }

    public void SetTerminalMode(int mode)
    {
        terminalMode = mode;
    }

    public int GetTerminalMode()
    {
        return terminalMode;
    }

    public bool IsConnected()
    {
        return isConnected;
    }

    public void Disconnect()
    {
        isConnected = false;
        Disconnected?.Invoke();
    }

    public void Clear()
    {
        textLines.Clear();
        AddLine("");
        cursorX = 0;
        cursorY = 0;
        scrollPosition = 0;
    }

    public void ClearHistory()
    {
        commandHistory.Clear();
        historyPosition = -1;
    }

    public List<string> GetTextLines()
    {
        return [..textLines];
    }

    public int GetCursorX()
    {
        return cursorX;
    }

    public int GetCursorY()
    {
        return cursorY;
    }

    public int GetHeight()
    {
        return textLines.Count;
    }

    public void SetCursorPosition(int x, int y)
    {
        cursorX = Math.Max(0, Math.Min(x, MAX_LINE_LENGTH - 1));
        cursorY = Math.Max(0, Math.Min(y, GetHeight() - 1));
    }

    public void SetForegroundColor(ConsoleColor color)
    {
        foregroundColor = color;
    }

    public void SetBackgroundColor(ConsoleColor color)
    {
        backgroundColor = color;
    }

    public ConsoleColor GetForegroundColor()
    {
        return foregroundColor;
    }

    public ConsoleColor GetBackgroundColor()
    {
        return backgroundColor;
    }

    public TerminalStatistics GetStatistics()
    {
        return new TerminalStatistics
        {
            LineCount = textLines.Count,
            CursorX = cursorX,
            CursorY = cursorY,
            IsConnected = isConnected,
            TerminalMode = terminalMode,
            LastActivity = lastActivity,
            CommandHistoryCount = commandHistory.Count
        };
    }

    public new bool IsVisible()
    {
        return isVisible;
    }

    public new bool IsEnabled()
    {
        return isEnabled;
    }

    public new void SetVisible(bool visible)
    {
        isVisible = visible;
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected new virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                textLines?.Clear();
                currentLine?.Clear();
                inputBuffer?.Clear();
                commandHistory?.Clear();
            }

            isDisposed = true;
        }
    }
        
    public override void Render(SpriteBatch spriteBatch)
    {
        // Implementation to render the terminal
    }
}
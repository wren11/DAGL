using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Chat;

public class ChatDisplayPane : ControlPane
{
    private readonly List<ChatMessage> _messages = [];
    private readonly List<TextSegment> _textSegments = [];
    private readonly Queue<ChatMessage> _messageQueue = new Queue<ChatMessage>();
    private readonly int _maxMessages = 100;
    private readonly int _maxDisplayLines = 20;
    private int _scrollOffset = 0;
    private bool _autoScroll = true;
    private bool _showTimestamps = true;
    private ChatFilter _currentFilter = ChatFilter.All;
    private Rectangle _displayArea;
    private Color _backgroundColor = Color.FromArgb(200, 0, 0, 0);
    private Color _borderColor = Color.Gray;

    public event EventHandler<ChatMessageEventArgs> MessageAdded;
    public event EventHandler<ChatMessageEventArgs> MessageRemoved;

    public ChatDisplayPane()
    {
        InitializeDisplay();
    }

    private void InitializeDisplay()
    {
        _displayArea = new Rectangle(10, 10, 600, 300);
    }

    public void AddMessage(ChatMessage message)
    {
        if (message == null)
            return;

        lock (_messageQueue)
        {
            _messageQueue.Enqueue(message);
        }

        ProcessMessageQueue();
    }

    public void AddMessage(string text, ChatType type = ChatType.System, string sender = "")
    {
        var message = new ChatMessage
        {
            Text = text,
            Type = type,
            Sender = sender,
            Timestamp = DateTime.Now
        };

        AddMessage(message);
    }

    private void ProcessMessageQueue()
    {
        lock (_messageQueue)
        {
            while (_messageQueue.Count > 0)
            {
                var message = _messageQueue.Dequeue();
                ProcessMessage(message);
            }
        }
    }

    private void ProcessMessage(ChatMessage message)
    {
        if (_messages.Count >= _maxMessages)
        {
            var removedMessage = _messages[0];
            _messages.RemoveAt(0);
            MessageRemoved?.Invoke(this, new ChatMessageEventArgs(removedMessage));
        }

        _messages.Add(message);
        MessageAdded?.Invoke(this, new ChatMessageEventArgs(message));

        if (_autoScroll)
        {
            ScrollToBottom();
        }

        UpdateTextSegments();
    }

    private void UpdateTextSegments()
    {
        _textSegments.Clear();

        var filteredMessages = GetFilteredMessages();
        var startIndex = Math.Max(0, filteredMessages.Count - _maxDisplayLines - _scrollOffset);
        var endIndex = Math.Min(filteredMessages.Count, startIndex + _maxDisplayLines);

        for (var i = startIndex; i < endIndex; i++)
        {
            var message = filteredMessages[i];
            var segments = CreateTextSegments(message);
            _textSegments.AddRange(segments);
        }
    }

    private List<ChatMessage> GetFilteredMessages()
    {
        if (_currentFilter == ChatFilter.All)
            return [.._messages];

        return _messages.FindAll(m => (ChatType)m.Type == (ChatType)_currentFilter);
    }

    private List<TextSegment> CreateTextSegments(ChatMessage message)
    {
        var segments = new List<TextSegment>();
        var font = FontManager.GetSimpleFont("default");

        if (_showTimestamps)
        {
            var timestamp = message.Timestamp.ToString("HH:mm:ss");
            segments.Add(new TextSegment
            {
                Text = $"[{timestamp}] ",
                Color = Color.Gray,
                Font = font
            });
        }

        if (!string.IsNullOrEmpty(message.Sender))
        {
            var senderColor = GetSenderColor(message.Type);
            segments.Add(new TextSegment
            {
                Text = $"{message.Sender}: ",
                Color = senderColor,
                Font = font
            });
        }

        var messageColor = GetMessageColor(message.Type);
        segments.Add(new TextSegment
        {
            Text = message.Text,
            Color = messageColor,
            Font = font
        });

        return segments;
    }

    private Color GetSenderColor(ChatType type)
    {
        return type switch
        {
            ChatType.Private => Color.Yellow,
            ChatType.Party => Color.Cyan,
            ChatType.Guild => Color.Green,
            ChatType.Trade => Color.Orange,
            ChatType.Shout => Color.Red,
            ChatType.System => Color.White,
            _ => Color.White
        };
    }

    private Color GetMessageColor(ChatType type)
    {
        return type switch
        {
            ChatType.Private => Color.Yellow,
            ChatType.Party => Color.Cyan,
            ChatType.Guild => Color.Green,
            ChatType.Trade => Color.Orange,
            ChatType.Shout => Color.Red,
            ChatType.System => Color.White,
            _ => Color.White
        };
    }

    public void ScrollUp()
    {
        if (_scrollOffset < _messages.Count - _maxDisplayLines)
        {
            _scrollOffset++;
            UpdateTextSegments();
        }
    }

    public void ScrollDown()
    {
        if (_scrollOffset > 0)
        {
            _scrollOffset--;
            UpdateTextSegments();
        }
    }

    public void ScrollToBottom()
    {
        _scrollOffset = 0;
        UpdateTextSegments();
    }

    public void SetFilter(ChatFilter filter)
    {
        _currentFilter = filter;
        UpdateTextSegments();
    }

    public void ToggleAutoScroll()
    {
        _autoScroll = !_autoScroll;
        if (_autoScroll)
        {
            ScrollToBottom();
        }
    }

    public void ToggleTimestamps()
    {
        _showTimestamps = !_showTimestamps;
        UpdateTextSegments();
    }

    public void ClearMessages()
    {
        _messages.Clear();
        _textSegments.Clear();
        _scrollOffset = 0;
    }

    public void SetDisplayArea(Rectangle area)
    {
        _displayArea = area;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        spriteBatch.FillRectangle(_displayArea, _backgroundColor);
        spriteBatch.DrawRectangle(_displayArea, _borderColor);

        var currentY = _displayArea.Y + 5;
        var maxY = _displayArea.Bottom - 5;

        foreach (var segment in _textSegments)
        {
            if (currentY >= maxY)
                break;

            //spriteBatch.DrawString(segment.Font, segment.Text, _displayArea.X + 5, currentY, segment.Color);
            //currentY += segment.Font.Height + 2;
        }

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (e is MouseEvent mouseEvent)
        {
            if (mouseEvent.Type == EventType.MouseWheel)
            {
                if (mouseEvent.Y > 0)
                    ScrollUp();
                else
                    ScrollDown();
                return true;
            }
        }

        if (e is KeyEvent keyEvent)
        {
            switch (keyEvent.Key)
            {
            case Silk.NET.Input.Key.PageUp:
                ScrollUp();
                return true;
            case Silk.NET.Input.Key.PageDown:
                ScrollDown();
                return true;
            case Silk.NET.Input.Key.Home:
                ScrollToBottom();
                return true;
            }
        }

        return base.HandleEvent(e);
    }

    public List<ChatMessage> GetMessages(ChatType type = ChatType.All)
    {
        if (type == ChatType.All)
            return [.._messages];

        return _messages.FindAll(m => m.Type == type);
    }

    public List<ChatMessage> GetMessagesBySender(string sender)
    {
        return _messages.FindAll(m => string.Equals(m.Sender, sender, StringComparison.OrdinalIgnoreCase));
    }

    public List<ChatMessage> GetMessagesByTimeRange(DateTime start, DateTime end)
    {
        return _messages.FindAll(m => m.Timestamp >= start && m.Timestamp <= end);
    }

    public void SaveToFile(string filename)
    {
        try
        {
            using var writer = new System.IO.StreamWriter(filename);
            foreach (var message in _messages)
            {
                writer.WriteLine($"[{message.Timestamp:yyyy-MM-dd HH:mm:ss}] [{message.Type}] {message.Sender}: {message.Text}");
            }
        }
        catch (Exception ex)
        {
            AddMessage($"Failed to save chat log: {ex.Message}", ChatType.System);
        }
    }
}
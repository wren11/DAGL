using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.GameLogic;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

public class BulletinBoardPane : DialogPane
{
    private readonly List<Article> _articles = [];
    private int _selectedIndex = -1;
    private int _scrollOffset = 0;
    private SimpleFont _font;
    private GraphicsDevice _graphicsDevice;
    private ImagePane _backgroundImage;
        
    private TextButtonExControlPane _readButton;
    private TextButtonExControlPane _writeButton;
    private TextButtonExControlPane _replyButton;
    private TextButtonExControlPane _closeButton;
        
    private const int ItemHeight = 25;
    private const int MaxVisibleItems = 15;

    public event EventHandler<Article> ArticleSelected;
    public event EventHandler ReadRequested;
    public event EventHandler WriteRequested;
    public event EventHandler ReplyRequested;

    public BulletinBoardPane()
    {
        LoadLayout();
        LoadArticles();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("lboard.txt");
                
            // Load background
            _backgroundImage = null;
            var backgroundName = layout.GetString("Noname", "Noname");
            var indexedImage = ImageLoader.LoadImage(backgroundName);
            if (indexedImage != null)
            {
                _backgroundImage = new ImagePane();
                _backgroundImage.SetImage(indexedImage, null);
            }
                
            // Create buttons
            _readButton = new TextButtonExControlPane("Read");
            _replyButton = new TextButtonExControlPane("Reply");
            _writeButton = new TextButtonExControlPane("Write");
            _closeButton = new TextButtonExControlPane("Close");
                
            // Wire up button events
            _readButton.Click += OnReadClicked;
            _replyButton.Click += OnReplyClicked;
            _writeButton.Click += OnWriteClicked;
            _closeButton.Click += OnCloseClicked;
                
            // Position buttons based on layout
            var readRect = layout.GetRect("ReadButton");
            var replyRect = layout.GetRect("ReplyButton");
            var writeRect = layout.GetRect("WriteButton");
            var closeRect = layout.GetRect("CloseButton");
                
            _readButton.Bounds = readRect;
            _replyButton.Bounds = replyRect;
            _writeButton.Bounds = writeRect;
            _closeButton.Bounds = closeRect;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading layout: {ex.Message}");
        }
    }

    private void LoadArticles()
    {
        try
        {
            var bulletinData = new BulletinDataFile("bulletin.dat");
            _articles.AddRange(bulletinData.GetArticles());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading articles: {ex.Message}");
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // Get graphics device and font if not already set
        if (_graphicsDevice == null)
            _graphicsDevice = GraphicsDevice.Instance;
        if (_font == null)
            _font = (SimpleFont?)FontManager.GetFont("default");

        // Render background
        if (_backgroundImage != null)
        {
            _backgroundImage.Render(spriteBatch);
        }
        else
        {
            spriteBatch.DrawRectangle(Bounds, Color.LightGray);
            spriteBatch.DrawRectangle(Bounds, Color.Black);
        }

        // Render article list
        var listX = Bounds.X + 10;
        var listY = Bounds.Y + 40;
        var listWidth = Bounds.Width - 20;
        var listHeight = MaxVisibleItems * ItemHeight;

        // List background
        spriteBatch.DrawRectangle(new Rectangle(listX, listY, listWidth, listHeight), Color.White);
        spriteBatch.DrawRectangle(new Rectangle(listX, listY, listWidth, listHeight), Color.Black);

        // Render visible articles
        var endIndex = Math.Min(_scrollOffset + MaxVisibleItems, _articles.Count);
        for (var i = _scrollOffset; i < endIndex; i++)
        {
            var y = listY + (i - _scrollOffset) * ItemHeight;
            var article = _articles[i];
                
            // Highlight selected item
            if (i == _selectedIndex)
            {
                spriteBatch.FillRectangle(
                    new Rectangle(listX, y, listWidth, ItemHeight),
                    Color.LightBlue
                );
            }

            // Render article title
            var displayText = $"{article.Title} - {article.Author} ({article.Date:MM/dd})";
        }

        // Render buttons
        _readButton?.Render(spriteBatch);
        _replyButton?.Render(spriteBatch);
        _writeButton?.Render(spriteBatch);
        _closeButton?.Render(spriteBatch);

        // Update button states
        var hasSelection = _selectedIndex >= 0 && _selectedIndex < _articles.Count;

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !IsEnabled) return false;

        // Handle button events
        if (_readButton?.HandleEvent(e) == true) return true;
        if (_replyButton?.HandleEvent(e) == true) return true;
        if (_writeButton?.HandleEvent(e) == true) return true;
        if (_closeButton?.HandleEvent(e) == true) return true;

        // Handle article list selection
        if (e is MouseEvent me)
        {
            var listX = Bounds.X + 10;
            var listY = Bounds.Y + 40;
            var listWidth = Bounds.Width - 20;
                
            if (me.X >= listX && me.X <= listX + listWidth &&
                me.Y >= listY && me.Y <= listY + MaxVisibleItems * ItemHeight)
            {
                if (me.Type == EventType.LButtonDown)
                {
                    var relativeY = me.Y - listY;
                    var clickedIndex = _scrollOffset + (relativeY / ItemHeight);
                        
                    if (clickedIndex >= 0 && clickedIndex < _articles.Count)
                    {
                        _selectedIndex = clickedIndex;
                        ArticleSelected?.Invoke(this, _articles[clickedIndex]);
                        return true;
                    }
                }
                else if (me.Type == EventType.MouseWheel)
                {
                    // Handle scrolling
                    if (me.Delta > 0 && _scrollOffset > 0)
                    {
                        _scrollOffset--;
                    }
                    else if (me.Delta < 0 && _scrollOffset + MaxVisibleItems < _articles.Count)
                    {
                        _scrollOffset++;
                    }
                    return true;
                }
            }
        }

        return base.HandleEvent(e);
    }

    private void OnReadClicked(object sender, EventArgs e)
    {
        ReadRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnReplyClicked(object sender, EventArgs e)
    {
        ReplyRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnWriteClicked(object sender, EventArgs e)
    {
        WriteRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        IsVisible = false;
    }

    public Article GetSelectedArticle()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _articles.Count)
            return _articles[_selectedIndex];
        return null;
    }

    public void RefreshArticles()
    {
        _articles.Clear();
        LoadArticles();
    }
}
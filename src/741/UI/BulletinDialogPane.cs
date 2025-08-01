using System;
using System.Drawing;
using DarkAges.Library.IO;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.GameLogic;

namespace DarkAges.Library.UI;

public class BulletinDialogPane : DialogPane
{
    private Article _currentArticle;
    private bool _isReplyMode;
    private string _replyToAuthor;
        
    private TextEditControlPane _titleLabel;
    private TextEditControlPane _authorLabel;
    private TextEditControlPane _dateLabel;
    private TextFlowPane _contentPane;
        
    private TextEditControlPane _titleInput;
    private TextEditControlPane _contentInput;
        
    private TextButtonExControlPane _replyButton;
    private TextButtonExControlPane _deleteButton;
    private TextButtonExControlPane _submitButton;
    private TextButtonExControlPane _closeButton;
        
    private SimpleFont _font;
    private GraphicsDevice _graphicsDevice;
    private ImagePane _backgroundImage;

    public event EventHandler<Article> ReplyRequested;
    public event EventHandler<Article> DeleteRequested;
    public event EventHandler<Article> SubmitRequested;

    public BulletinDialogPane(Article article = null)
    {
        _currentArticle = article;
        LoadLayout();
        SetupControls();
    }

    public BulletinDialogPane(string replyToAuthor, Article originalArticle)
    {
        _isReplyMode = true;
        _replyToAuthor = replyToAuthor;
        _currentArticle = originalArticle;
        LoadLayout();
        SetupControls();
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("bulletin_dialog.txt");
                
            // Load background
            var backgroundName = layout.GetString("Noname", "Noname");
            var indexedImage = ImageLoader.LoadImage(backgroundName);
            if (indexedImage != null)
            {
                _backgroundImage = new ImagePane();
                _backgroundImage.SetImage(indexedImage, null!);
            }
        }
        catch
        {
            // Fallback background
            _backgroundImage = null;
        }
    }

    private void SetupControls()
    {
        // Create controls based on mode
        if (_isReplyMode)
        {
            // Reply mode - input controls
            _titleInput = new TextEditControlPane("Re: " + (_currentArticle?.Title ?? ""), new Rectangle(10, 50, 400, 25));
            _contentInput = new TextEditControlPane("", new Rectangle(10, 90, 400, 200));
                
            _submitButton = new TextButtonExControlPane("Submit");
            _submitButton.Click += OnSubmitClicked;
                
            _closeButton = new TextButtonExControlPane("Cancel");
            _closeButton.Click += OnCloseClicked;
        }
        else
        {
            // View mode - display controls
            _titleLabel = new TextEditControlPane(_currentArticle?.Title ?? "", new Rectangle(10, 20, 400, 25), true);
            _authorLabel = new TextEditControlPane(_currentArticle?.Author ?? "", new Rectangle(10, 50, 200, 25), true);
            _dateLabel = new TextEditControlPane(_currentArticle?.Date.ToString("MM/dd/yyyy HH:mm") ?? "", new Rectangle(220, 50, 200, 25), true);
                
            _contentPane = new TextFlowPane(new Rectangle(10, 90, 400, 200));
            _contentPane.Text = _currentArticle?.Content ?? "";
                
            _replyButton = new TextButtonExControlPane("Reply");
            _replyButton.Click += OnReplyClicked;
                
            _deleteButton = new TextButtonExControlPane("Delete");
            _deleteButton.Click += OnDeleteClicked;
                
            _closeButton = new TextButtonExControlPane("Close");
            _closeButton.Click += OnCloseClicked;
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

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

        // Render title
        var title = _isReplyMode ? "Reply to Article" : "Article";
        //spriteBatch.DrawString(_font, title, Bounds.X + 10, Bounds.Y + 5, Color.Black);

        // Render controls
        if (_isReplyMode)
        {
            _titleInput?.Render(spriteBatch);
            _contentInput?.Render(spriteBatch);
            _submitButton?.Render(spriteBatch);
            _closeButton?.Render(spriteBatch);
        }
        else
        {
            _titleLabel?.Render(spriteBatch);
            _authorLabel?.Render(spriteBatch);
            _dateLabel?.Render(spriteBatch);
            _contentPane?.Render(spriteBatch);
            _replyButton?.Render(spriteBatch);
            _deleteButton?.Render(spriteBatch);
            _closeButton?.Render(spriteBatch);
        }

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || !IsEnabled) return false;

        // Handle control events
        if (_isReplyMode)
        {
            if (_titleInput?.HandleEvent(e) == true) return true;
            if (_contentInput?.HandleEvent(e) == true) return true;
            if (_submitButton?.HandleEvent(e) == true) return true;
            if (_closeButton?.HandleEvent(e) == true) return true;
        }
        else
        {
            if (_titleLabel?.HandleEvent(e) == true) return true;
            if (_authorLabel?.HandleEvent(e) == true) return true;
            if (_dateLabel?.HandleEvent(e) == true) return true;
            if (_contentPane?.HandleEvent(e) == true) return true;
            if (_replyButton?.HandleEvent(e) == true) return true;
            if (_deleteButton?.HandleEvent(e) == true) return true;
            if (_closeButton?.HandleEvent(e) == true) return true;
        }

        return base.HandleEvent(e);
    }

    private void OnReplyClicked(object sender, EventArgs e)
    {
        ReplyRequested?.Invoke(this, _currentArticle);
    }

    private void OnDeleteClicked(object sender, EventArgs e)
    {
        DeleteRequested?.Invoke(this, _currentArticle);
    }

    private void OnSubmitClicked(object sender, EventArgs e)
    {
        if (_isReplyMode && _currentArticle != null)
        {
            var replyArticle = new Article
            {
                Title = _titleInput.Text,
                Content = _contentInput.Text,
                Author = "CurrentUser", // This should come from user session
                Date = DateTime.Now,
                ParentId = _currentArticle.Id
            };
                
            SubmitRequested?.Invoke(this, replyArticle);
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        IsVisible = false;
    }

    public void SetArticle(Article article)
    {
        _currentArticle = article;
        if (!_isReplyMode)
        {
            _titleLabel.Text = article?.Title ?? "";
            _authorLabel.Text = article?.Author ?? "";
            _dateLabel.Text = article?.Date.ToString("MM/dd/yyyy HH:mm") ?? "";
            _contentPane.Text = article?.Content ?? "";
        }
    }

    public Article GetCurrentArticle()
    {
        return _currentArticle;
    }
}
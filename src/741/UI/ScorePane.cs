using System;
using System.Drawing;
using System.Text;
using DarkAges.Library.Graphics;
using DarkAges.Library.Core.Events;
using DarkAges.Library.IO;

namespace DarkAges.Library.UI;

/// <summary>
/// Handles score display and management for mini-games and competitions
/// </summary>
public class ScorePane : Pane
{
    private const int MAX_SCORES = 100;
    private const int SCORE_ENTRY_SIZE = 12;
    private const int DEFAULT_WIDTH = 300;
    private const int DEFAULT_HEIGHT = 400;
    private const int TEXT_PADDING = 5;
    private const int LINE_HEIGHT = 16;

    private ScoreEntry[] scores;
    private int scoreCount;
    private int maxScores;
    private int currentScore;
    private int displayStartIndex;
    private int displayCount;
    private bool isScrolling;
    private float scrollOffset;
    private float scrollSpeed;
    private System.Drawing.Color backgroundColor;
    private System.Drawing.Color textColor;
    private System.Drawing.Color highlightColor;
    private System.Drawing.Color borderColor;
    private Font scoreFont;
    private Font titleFont;
    private string title;
    private bool showTitle;
    private int titleHeight;

    // Animation properties
    private bool isAnimating;
    private float animationTime;
    private float animationDuration;
    private int animationType; // 0 = fade, 1 = slide, 2 = scale

    // Events
    public event Action<int> ScoreSelected;
    public event Action<int> ScoreUpdated;

    public ScorePane()
    {
        InitializeScorePane();
        LoadLayout();
    }

    public ScorePane(int maxScores, Rectangle bounds)
    {
        InitializeScorePane();
        LoadLayout();
        SetMaxScores(maxScores);
        SetBounds(bounds);
    }

    private void InitializeScorePane()
    {
        // Initialize score storage
        scores = new ScoreEntry[MAX_SCORES];
        scoreCount = 0;
        maxScores = MAX_SCORES;
        currentScore = 0;
        displayStartIndex = 0;
        displayCount = 0;
            
        // Scrolling properties
        isScrolling = false;
        scrollOffset = 0.0f;
        scrollSpeed = 0.0f;
            
        // Animation
        isAnimating = false;
        animationTime = 0.0f;
        animationDuration = 0.5f;
        animationType = 0;
    }

    private void LoadLayout()
    {
        try
        {
            var layout = new LayoutFileParser("_score.txt");
            Bounds = layout.GetRect("Bounds", new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT));
            backgroundColor = layout.GetColor("BackgroundColor", Color.FromArgb(255, 32, 32, 32));
            textColor = layout.GetColor("TextColor", Color.White);
            highlightColor = layout.GetColor("HighlightColor", Color.FromArgb(255, 255, 255, 0));
            borderColor = layout.GetColor("BorderColor", Color.FromArgb(255, 128, 128, 128));
            var scoreFontName = layout.GetString("ScoreFontName", "Arial");
            var scoreFontSize = layout.GetInt("ScoreFontSize", 10);
            scoreFont = new Font(scoreFontName, scoreFontSize);
            var titleFontName = layout.GetString("TitleFontName", "Arial");
            var titleFontSize = layout.GetInt("TitleFontSize", 12);
            var titleFontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), layout.GetString("TitleFontStyle", "Bold"));
            titleFont = new Font(titleFontName, titleFontSize, titleFontStyle);
            title = layout.GetString("Title", "Scores");
            showTitle = layout.GetBool("ShowTitle", true);
            titleHeight = layout.GetInt("TitleHeight", 25);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading score pane layout: {ex.Message}");
            // Fallback to defaults
            Bounds = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            backgroundColor = Color.FromArgb(255, 32, 32, 32);
            textColor = Color.White;
            highlightColor = Color.FromArgb(255, 255, 255, 0);
            borderColor = Color.FromArgb(255, 128, 128, 128);
            scoreFont = new Font("Arial", 10);
            titleFont = new Font("Arial", 12, FontStyle.Bold);
            title = "Scores";
            showTitle = true;
            titleHeight = 25;
        }
    }

    public void SetMaxScores(int max)
    {
        maxScores = Math.Max(1, Math.Min(max, MAX_SCORES));
        if (scoreCount > maxScores)
        {
            // Remove excess scores (keep highest)
            Array.Sort(scores, 0, scoreCount, new ScoreEntryComparer());
            scoreCount = maxScores;
        }
    }

    public int GetMaxScores()
    {
        return maxScores;
    }

    public void AddScore(string playerName, int score, DateTime timestamp)
    {
        if (scoreCount >= maxScores)
        {
            // Find lowest score to replace
            var lowestIndex = 0;
            for (var i = 1; i < scoreCount; i++)
            {
                if (scores[i].Score < scores[lowestIndex].Score)
                {
                    lowestIndex = i;
                }
            }
                
            // Only replace if new score is higher
            if (score > scores[lowestIndex].Score)
            {
                scores[lowestIndex] = new ScoreEntry
                {
                    PlayerName = playerName,
                    Score = score,
                    Timestamp = timestamp
                };
            }
        }
        else
        {
            scores[scoreCount] = new ScoreEntry
            {
                PlayerName = playerName,
                Score = score,
                Timestamp = timestamp
            };
            scoreCount++;
        }

        // Sort scores (highest first)
        Array.Sort(scores, 0, scoreCount, new ScoreEntryComparer());
            
        // Trigger event
        ScoreUpdated?.Invoke(score);
    }

    public void AddScore(string playerName, int score)
    {
        AddScore(playerName, score, DateTime.Now);
    }

    public void RemoveScore(int index)
    {
        if (index >= 0 && index < scoreCount)
        {
            // Shift remaining scores
            for (var i = index; i < scoreCount - 1; i++)
            {
                scores[i] = scores[i + 1];
            }
            scoreCount--;
        }
    }

    public void ClearScores()
    {
        scoreCount = 0;
        displayStartIndex = 0;
    }

    public ScoreEntry? GetScore(int index)
    {
        if (index >= 0 && index < scoreCount)
        {
            return scores[index];
        }
        return null;
    }

    public int GetScoreCount()
    {
        return scoreCount;
    }

    public void SetCurrentScore(int score)
    {
        currentScore = score;
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void SetTitle(string title)
    {
        this.title = title ?? "Scores";
        showTitle = !string.IsNullOrEmpty(title);
    }

    public string GetTitle()
    {
        return title;
    }

    public void SetColors(System.Drawing.Color background, System.Drawing.Color text, System.Drawing.Color highlight, System.Drawing.Color border)
    {
        backgroundColor = background;
        textColor = text;
        highlightColor = highlight;
        borderColor = border;
    }

    public void SetFonts(Font scoreFont, Font titleFont)
    {
        this.scoreFont?.Dispose();
        this.titleFont?.Dispose();
            
        this.scoreFont = scoreFont;
        this.titleFont = titleFont;
    }

    public void StartAnimation(int type, float duration)
    {
        isAnimating = true;
        animationTime = 0.0f;
        animationDuration = duration;
        animationType = type;
    }

    public void StopAnimation()
    {
        isAnimating = false;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Update animation
        if (isAnimating)
        {
            animationTime += deltaTime;
            if (animationTime >= animationDuration)
            {
                isAnimating = false;
                animationTime = animationDuration;
            }
        }

        // Update scrolling
        if (isScrolling)
        {
            scrollOffset += scrollSpeed * deltaTime;
                
            // Clamp scroll offset
            float maxScroll = Math.Max(0, scoreCount - displayCount);
            if (scrollOffset > maxScroll)
            {
                scrollOffset = maxScroll;
                isScrolling = false;
            }
            else if (scrollOffset < 0)
            {
                scrollOffset = 0;
                isScrolling = false;
            }
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        // Calculate display area
        var displayY = Bounds.Y + (showTitle ? titleHeight : 0);
        var displayHeight = Bounds.Height - (showTitle ? titleHeight : 0);
        displayCount = displayHeight / LINE_HEIGHT;

        // Draw background
        spriteBatch.FillRectangle(Bounds, backgroundColor);

        // Draw border
        spriteBatch.DrawRectangle(Bounds, borderColor);

        // Draw title
        if (showTitle)
        {
            //spriteBatch.DrawString(titleFont, title, 
            //                      new Point(Bounds.X + TEXT_PADDING, Bounds.Y + TEXT_PADDING), textColor);
        }

        // Draw scores
        var startIndex = (int)scrollOffset;
        var endIndex = Math.Min(startIndex + displayCount, scoreCount);

        for (var i = startIndex; i < endIndex; i++)
        {
            var entry = scores[i];
            var y = displayY + (i - startIndex) * LINE_HEIGHT;
                
            // Highlight current player's score
            var entryTextColor = textColor;
            if (i == currentScore)
            {
                spriteBatch.FillRectangle(new Rectangle(Bounds.X, y, Bounds.Width, LINE_HEIGHT), highlightColor);
                entryTextColor = System.Drawing.Color.Black;
            }

            // Draw score text
            var scoreText = $"{i + 1:D2}. {entry.PlayerName} - {entry.Score:D6}";
            //spriteBatch.DrawString(scoreFont, scoreText, 
            //                      new Point(Bounds.X + TEXT_PADDING, y + 2), entryTextColor);
        }

        // Draw scroll indicators if needed
        if (scoreCount > displayCount)
        {
            DrawScrollIndicators(spriteBatch, displayY, displayHeight);
        }
    }

    private void DrawScrollIndicators(SpriteBatch spriteBatch, int displayY, int displayHeight)
    {
        // Draw up arrow if scrolled down
        if (scrollOffset > 0)
        {
            var arrowX = Bounds.Right - 15;
            var arrowY = displayY + 5;
                    
            //spriteBatch.DrawLine(new Point(arrowX, arrowY + 5), new Point(arrowX, arrowY), textColor);
            //spriteBatch.DrawLine(new Point(arrowX - 3, arrowY + 2), new Point(arrowX, arrowY + 5), textColor);
            //spriteBatch.DrawLine(new Point(arrowX + 3, arrowY + 2), new Point(arrowX, arrowY + 5), textColor);
        }

        // Draw down arrow if more scores below
        if (scrollOffset + displayCount < scoreCount)
        {
            var arrowX = Bounds.Right - 15;
            var arrowY = displayY + displayHeight - 10;
                    
            //spriteBatch.DrawLine(new Point(arrowX, arrowY), new Point(arrowX, arrowY + 5), textColor);
            //spriteBatch.DrawLine(new Point(arrowX - 3, arrowY + 3), new Point(arrowX, arrowY), textColor);
            //spriteBatch.DrawLine(new Point(arrowX + 3, arrowY + 3), new Point(arrowX, arrowY), textColor);
        }
    }

    public override bool HandleEvent(Event e)
    {
        if (e is MouseEvent mouseEvent && Bounds.Contains(mouseEvent.X, mouseEvent.Y))
        {
            switch (mouseEvent.Type)
            {
            case EventType.MouseDown:
                HandleScoreSelection(new Point(mouseEvent.X, mouseEvent.Y));
                return true;

            case EventType.MouseWheel:
                HandleScrollWheel(mouseEvent.Delta);
                return true;
            }
        }
        else if (e is KeyEvent keyEvent)
        {
            if (keyEvent.Type == EventType.KeyDown)
            {
                switch (keyEvent.Key)
                {
                case Silk.NET.Input.Key.Up:
                    ScrollUp();
                    return true;
                case Silk.NET.Input.Key.Down:
                    ScrollDown();
                    return true;
                case Silk.NET.Input.Key.PageUp:
                    ScrollPageUp();
                    return true;
                case Silk.NET.Input.Key.PageDown:
                    ScrollPageDown();
                    return true;
                case Silk.NET.Input.Key.Home:
                    ScrollToTop();
                    return true;
                case Silk.NET.Input.Key.End:
                    ScrollToBottom();
                    return true;
                }
            }
        }

        return base.HandleEvent(e);
    }

    private void HandleScoreSelection(Point mousePos)
    {
        var displayY = Bounds.Y + (showTitle ? titleHeight : 0);
        var displayHeight = Bounds.Height - (showTitle ? titleHeight : 0);
        displayCount = displayHeight / LINE_HEIGHT;

        var relativeY = mousePos.Y - displayY;
        var scoreIndex = (int)scrollOffset + (relativeY / LINE_HEIGHT);

        if (scoreIndex >= 0 && scoreIndex < scoreCount)
        {
            currentScore = scoreIndex;
            ScoreSelected?.Invoke(scoreIndex);
        }
    }

    private void HandleScrollWheel(int wheelDelta)
    {
        if (wheelDelta > 0)
        {
            ScrollUp();
        }
        else
        {
            ScrollDown();
        }
    }

    public void ScrollUp()
    {
        if (scrollOffset > 0)
        {
            scrollOffset--;
            isScrolling = true;
        }
    }

    public void ScrollDown()
    {
        var displayHeight = Bounds.Height - (showTitle ? titleHeight : 0);
        displayCount = displayHeight / LINE_HEIGHT;
            
        if (scrollOffset + displayCount < scoreCount)
        {
            scrollOffset++;
            isScrolling = true;
        }
    }

    public void ScrollPageUp()
    {
        var displayHeight = Bounds.Height - (showTitle ? titleHeight : 0);
        displayCount = displayHeight / LINE_HEIGHT;
            
        scrollOffset = Math.Max(0, scrollOffset - displayCount);
        isScrolling = true;
    }

    public void ScrollPageDown()
    {
        var displayHeight = Bounds.Height - (showTitle ? titleHeight : 0);
        displayCount = displayHeight / LINE_HEIGHT;
            
        scrollOffset = Math.Min(scoreCount - displayCount, scrollOffset + displayCount);
        isScrolling = true;
    }

    public void ScrollToTop()
    {
        scrollOffset = 0;
        isScrolling = true;
    }

    public void ScrollToBottom()
    {
        var displayHeight = Bounds.Height - (showTitle ? titleHeight : 0);
        displayCount = displayHeight / LINE_HEIGHT;
            
        scrollOffset = Math.Max(0, scoreCount - displayCount);
        isScrolling = true;
    }

    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }

    public float GetScrollOffset()
    {
        return scrollOffset;
    }

    public void SetScrollOffset(float offset)
    {
        scrollOffset = Math.Max(0, offset);
    }

    public bool IsScrolling()
    {
        return isScrolling;
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }

    public ScoreEntry[] GetTopScores(int count)
    {
        count = Math.Min(count, scoreCount);
        var topScores = new ScoreEntry[count];
        Array.Copy(scores, topScores, count);
        return topScores;
    }

    public int GetPlayerRank(string playerName)
    {
        for (var i = 0; i < scoreCount; i++)
        {
            if (scores[i].PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
            {
                return i + 1;
            }
        }
        return -1; // Player not found
    }

    public ScoreEntry GetPlayerScore(string playerName)
    {
        for (var i = 0; i < scoreCount; i++)
        {
            if (scores[i].PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
            {
                return scores[i];
            }
        }
        return default;
    }

    public override void Dispose()
    {
        //scoreFont?.Dispose();
        //titleFont?.Dispose();
        base.Dispose();
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.FindFarmpet;

public class FindFarmpetPane : ControlPane
{
    private readonly List<Farmpet> _farmpets = [];
    private readonly List<Farmpet> _foundFarmpets = [];
    private int _score = 0;
    private int _timeRemaining = 120;
    private bool _isGameOver = false;
    private DateTime _lastUpdate = DateTime.Now;

    private TextButtonExControlPane _startButton;
    private TextButtonExControlPane _exitButton;
    private TextEditControlPane _scoreLabel;
    private TextEditControlPane _timeLabel;

    public event EventHandler<int> GameCompleted;

    public FindFarmpetPane()
    {
        InitializeUI();
        InitializeFarmpets();
    }

    private void InitializeUI()
    {
        _startButton = new TextButtonExControlPane("Start");
        _exitButton = new TextButtonExControlPane("Exit");
        _scoreLabel = new TextEditControlPane("Score: 0", new Rectangle(400, 50, 150, 25), true);
        _timeLabel = new TextEditControlPane("Time: 120", new Rectangle(400, 80, 150, 25), true);

        _startButton.Position = new Point(200, 150);
        _exitButton.Position = new Point(200, 180);

        AddChild(_startButton);
        AddChild(_exitButton);
        AddChild(_scoreLabel);
        AddChild(_timeLabel);

        _startButton.Click += (s, e) => StartGame();
    }

    private void InitializeFarmpets()
    {
        var random = new Random();
        var farmpetTypes = new[] { "Dog", "Cat", "Horse", "Cow", "Pig", "Sheep", "Chicken", "Duck" };

        for (var i = 0; i < 8; i++)
        {
            var farmpet = new Farmpet
            {
                Id = i,
                Type = farmpetTypes[i],
                X = random.Next(50, 350),
                Y = random.Next(50, 250),
                Width = 40,
                Height = 40,
                IsFound = false
            };
            _farmpets.Add(farmpet);
        }
    }

    private void StartGame()
    {
        _score = 0;
        _timeRemaining = 120;
        _isGameOver = false;
        _foundFarmpets.Clear();

        foreach (var farmpet in _farmpets)
        {
            farmpet.IsFound = false;
        }

        UpdateUI();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        //var graphicsDevice = GraphicsDevice.Instance;
        //var font = FontManager.Instance.GetFont("default");

        if (!_isGameOver)
        {
            //spriteBatch.DrawString(font, "Find Farmpet Game", 150, 20, Color.Black);

            foreach (var farmpet in _farmpets)
            {
                if (!farmpet.IsFound)
                {
                    var rect = new Rectangle(farmpet.X, farmpet.Y, farmpet.Width, farmpet.Height);
                    spriteBatch.DrawRectangle(rect, Color.Green);
                    //spriteBatch.DrawString(font, farmpet.Type, farmpet.X, farmpet.Y - 20, Color.Black);
                }
            }
        }
        else
        {
            //spriteBatch.DrawString(font, "Game Over!", 200, 100, Color.Black);
            //spriteBatch.DrawString(font, $"Final Score: {_score}", 200, 130, Color.Black);
        }
            
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible || _isGameOver) return false;

        if (e is MouseEvent me && me.Type == EventType.LButtonDown)
        {
            foreach (var farmpet in _farmpets)
            {
                if (!farmpet.IsFound)
                {
                    var rect = new Rectangle(farmpet.X, farmpet.Y, farmpet.Width, farmpet.Height);
                    if (rect.Contains(me.X, me.Y))
                    {
                        farmpet.IsFound = true;
                        _foundFarmpets.Add(farmpet);
                        _score += 10;
                        UpdateUI();

                        if (_foundFarmpets.Count >= _farmpets.Count)
                        {
                            EndGame();
                        }
                        return true;
                    }
                }
            }
        }

        return base.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
        if (_isGameOver) return;

        var now = DateTime.Now;
        if ((now - _lastUpdate).TotalMilliseconds > 1000)
        {
            _timeRemaining--;
            _lastUpdate = now;
            UpdateUI();

            if (_timeRemaining <= 0)
            {
                EndGame();
            }
        }
    }

    private void UpdateUI()
    {
        _scoreLabel.Text = $"Score: {_score}";
        _timeLabel.Text = $"Time: {_timeRemaining}";
    }

    private void EndGame()
    {
        _isGameOver = true;
        GameCompleted?.Invoke(this, _score);
    }

    public override void Dispose()
    {
        _farmpets.Clear();
        _foundFarmpets.Clear();
        base.Dispose();
    }
}
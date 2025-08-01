using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using DarkAges.Library.UI.Puzzle;
using DarkAges.Library.UI.RopeSkipping;

namespace DarkAges.Library.UI.MiniGame;

public class MiniGameSystem : ControlPane
{
    private readonly List<AbstractGame> _games = [];
    private AbstractGame _currentGame = null!;
    private GameMenu _gameMenu;
    private TimerEventMan _timerManager;
    private bool _isInitialized;

    public event EventHandler<AbstractGame> GameStarted = null!;
    public event EventHandler<AbstractGame> GameEnded = null!;

    public MiniGameSystem()
    {
        _timerManager = new TimerEventMan();
        _gameMenu = new GameMenu();
        InitializeGames();
    }

    private void InitializeGames()
    {
        _games.Add(new PuzzleGame());
        _games.Add(new RopeSkippingGame());
        _isInitialized = true;
    }

    public void StartGame(int gameIndex)
    {
        if (gameIndex >= 0 && gameIndex < _games.Count)
        {
            _currentGame = _games[gameIndex];
            _currentGame.Initialize();
            GameStarted?.Invoke(this, _currentGame);
        }
    }

    public void EndCurrentGame()
    {
        if (_currentGame != null)
        {
            GameEnded?.Invoke(this, _currentGame);
            _currentGame = null;
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        if (_currentGame != null)
        {
            _currentGame.Render(spriteBatch);
        }
        else
        {
            _gameMenu.Render(spriteBatch);
        }

        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_currentGame != null)
        {
            return _currentGame.HandleEvent(e);
        }
        else
        {
            return _gameMenu.HandleEvent(e);
        }
    }

    public override void Update(float deltaTime)
    {
        _timerManager.Update(deltaTime);
            
        if (_currentGame != null)
        {
            _currentGame.Update(deltaTime);
        }
        else
        {
            _gameMenu.Update(deltaTime);
        }
    }

    public override void Dispose()
    {
        foreach (var game in _games)
        {
            game?.Dispose();
        }
        _games.Clear();
        _timerManager?.Dispose();
        _gameMenu?.Dispose();
    }
}
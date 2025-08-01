using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;
using System.Drawing;
using DarkAges.Library.UI.MiniGame;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzleGame : AbstractGame
{
    private readonly List<PuzzleGameState> _states = [];
    private PuzzleGameState _currentState;
    private int _currentStateIndex;
    private bool _isNetworkMode;
    private bool _isPaused;
    private bool _isGameOver;
    private int _score;
    private int _level;
        
    private PuzzleTitleState _titleState;
    private PuzzlePlayState _playState;
    private PuzzleResultState _resultState;
    private PuzzleSendResultState _sendResultState;
    private PuzzleSettingState _settingState;
    private PuzzleRankingState _rankingState;
    private PuzzleHelpState _helpState;

    public event EventHandler<int> GameStateChanged;
    public event EventHandler<int> GameCompleted;

    public PuzzleGame(bool isNetworkMode = false)
    {
        _isNetworkMode = isNetworkMode;
        _currentStateIndex = 0;
        _isPaused = false;
        _isGameOver = false;
            
        InitializeStates();
        SetState(0);
    }

    public override void Initialize()
    {
        _isInitialized = true;
    }

    public override void Start()
    {
        _isRunning = true;
        SetState(1);
    }

    public override void Pause()
    {
        _isPaused = true;
    }

    public override void Resume()
    {
        _isPaused = false;
    }

    public override void End()
    {
        _isGameOver = true;
        _isRunning = false;
        GameCompleted?.Invoke(this, _score);
    }

    public override void Reset()
    {
        _isGameOver = false;
        _isPaused = false;
        _score = 0;
        _level = 1;
        SetState(0);
    }

    private void InitializeStates()
    {
        _titleState = new PuzzleTitleState(this);
        _playState = new PuzzlePlayState(this);
        _resultState = new PuzzleResultState(this);
        _sendResultState = new PuzzleSendResultState(this);
        _settingState = new PuzzleSettingState(this);
        _rankingState = new PuzzleRankingState(this);
        _helpState = new PuzzleHelpState(this);

        _states.Add(_titleState);
        _states.Add(_playState);
        _states.Add(_resultState);
        _states.Add(_sendResultState);
        _states.Add(_settingState);
        _states.Add(_rankingState);
        _states.Add(_helpState);
    }

    public void SetState(int stateIndex)
    {
        if (stateIndex >= 0 && stateIndex < _states.Count)
        {
            _currentStateIndex = stateIndex;
            _currentState = _states[stateIndex];
            _currentState.Initialize();
            GameStateChanged?.Invoke(this, stateIndex);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _currentState?.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_currentState?.HandleEvent(e) == true)
            return true;

        return base.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
        if (_isPaused || _isGameOver) return;

        _currentState?.Update(deltaTime);
    }

    public override void Dispose()
    {
        foreach (var state in _states)
        {
            state?.Dispose();
        }
        _states.Clear();
    }
}
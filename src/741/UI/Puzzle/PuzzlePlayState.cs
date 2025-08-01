using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Puzzle;

public class PuzzlePlayState(PuzzleGame game) : PuzzleGameState(game)
{
    private PuzzlePlayPane _playPane = new();
    private PuzzleGameControlPane _controlPane = new();
    private int _score;
    private int _level;
    private int _timeRemaining;

    public override void Initialize()
    {
        _score = 0;
        _level = 1;
        _timeRemaining = 300;
            
        _playPane.Initialize(_level);
        _controlPane.UpdateScore(_score);
        _controlPane.UpdateLevel(_level);
        _controlPane.UpdateTime(_timeRemaining);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        _playPane.Render(spriteBatch);
        _controlPane.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (_playPane.HandleEvent(e))
            return true;
            
        return _controlPane.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
        _playPane.Update(deltaTime);
            
        if (_playPane.IsCompleted)
        {
            _score += _playPane.GetScore();
            _level++;
            _playPane.Initialize(_level);
            _controlPane.UpdateScore(_score);
            _controlPane.UpdateLevel(_level);
        }
            
        if (_timeRemaining <= 0)
        {
            _game.SetState(2);
        }
    }

    public override void Dispose()
    {
        _playPane?.Dispose();
        _controlPane?.Dispose();
    }
}
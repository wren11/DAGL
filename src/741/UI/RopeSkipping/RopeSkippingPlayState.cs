using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.RopeSkipping;

public class RopeSkippingPlayState(RopeSkippingGame game) : RopeSkippingGameState(game)
{
    private RopeSkippingPlayPane _playPane = new();
    private RopeSkippingGameControlPane _controlPane = new();
    private RopeSkippingSkipCountPane _skipCountPane = new();
    private int _score;
    private int _skipCount;
    private int _timeRemaining;
    private DateTime _lastUpdate = DateTime.Now;

    public override void Initialize()
    {
        _score = 0;
        _skipCount = 0;
        _timeRemaining = 60;
            
        _playPane.Initialize();
        _controlPane.UpdateScore(_score);
        _controlPane.UpdateTime(_timeRemaining);
        _skipCountPane.UpdateCount(_skipCount);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        _playPane.Render(spriteBatch);
        _controlPane.Render(spriteBatch);
        _skipCountPane.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (_playPane.HandleEvent(e))
            return true;
            
        if (_controlPane.HandleEvent(e))
            return true;

        return _skipCountPane.HandleEvent(e);
    }

    public override void Update(float deltaTime)
    {
        var now = DateTime.Now;
        if ((now - _lastUpdate).TotalMilliseconds > 1000)
        {
            _timeRemaining--;
            _controlPane.UpdateTime(_timeRemaining);
            _lastUpdate = now;

            if (_timeRemaining <= 0)
            {
                _game.SetState(2);
            }
        }

        _playPane.Update(deltaTime);
            
        if (_playPane.JumpDetected)
        {
            _skipCount++;
            _score += 10;
            _skipCountPane.UpdateCount(_skipCount);
            _controlPane.UpdateScore(_score);
            _playPane.ResetJumpDetection();
        }
    }

    public override void Dispose()
    {
        _playPane?.Dispose();
        _controlPane?.Dispose();
        _skipCountPane?.Dispose();
    }
}
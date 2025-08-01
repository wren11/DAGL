using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.SafeQuit;

public class SafeQuitSystem : ControlPane
{
    private readonly SafeQuitAlert _quitAlert;
    private bool _quitRequested;
    private bool _isDisposed;

    public event EventHandler? QuitConfirmed;
    public event EventHandler? QuitCancelled;

    public SafeQuitSystem()
    {
        _quitAlert = new SafeQuitAlert();
        _quitAlert.QuitConfirmed += (s, confirmed) => HandleQuitConfirmation(confirmed);
    }

    public void RequestQuit()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(SafeQuitSystem));
            
        _quitRequested = true;
        _quitAlert.ShowQuitDialog();
    }

    private void HandleQuitConfirmation(bool confirmed)
    {
        if (_isDisposed)
            return;
            
        if (confirmed)
        {
            QuitConfirmed?.Invoke(this, EventArgs.Empty);
            PerformQuit();
        }
        else
        {
            QuitCancelled?.Invoke(this, EventArgs.Empty);
            _quitRequested = false;
        }
    }

    private void PerformQuit()
    {
        try
        {
            SaveGameState();
            CleanupResources();
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during quit: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private void SaveGameState()
    {
        try
        {
            var gameState = new GameState();
            gameState.SaveToFile("autosave.dat");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game state: {ex.Message}");
        }
    }

    private void CleanupResources()
    {
        try
        {
            GraphicsDevice.Instance?.Dispose();
            FontManager.Dispose();
            ImageLoader.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up resources: {ex.Message}");
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (_isDisposed || !IsVisible || spriteBatch == null) 
            return;

        _quitAlert.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (_isDisposed || !IsVisible || e == null) 
            return false;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        if (_isDisposed)
            return;

        _quitAlert.Dispose();
        _isDisposed = true;
        base.Dispose();
    }
}
using System;
using System.Drawing;
using System.Numerics;
using DarkAges.Library.Common;
using DarkAges.Library.Graphics;
using DarkAges.Library.UI;
using DarkAges.Library.UI.Connection;
using DarkAges.Library.UI.Users;
using DarkAges.Library.UI.Dialogs;
using DarkAges.Library.UI.ItemShop;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using EventManager = DarkAges.Library.Core.Events.EventManager;
using InputManager = DarkAges.Library.Input.InputManager;
using NetworkManager = DarkAges.Library.Network.NetworkManager;
using ResourceManager = DarkAges.Library.IO.ResourceManager;
using ScreenManager = DarkAges.Library.UI.ScreenManager;
using SoundManager = DarkAges.Library.Audio.SoundManager;

namespace DarkAges.Library.Core;

public class DarkAgesApplication : IApplication, IDisposable
{
    private const int WINDOW_WIDTH = 800;
    private const int WINDOW_HEIGHT = 600;
    private const string WINDOW_TITLE = "Dark Ages";

    private IWindow? _window;
    private GL? _gl;
    private GraphicsDevice? _graphicsDevice;
    private SpriteBatch? _spriteBatch;
    private bool _isInitialized;
    private bool _isRunning;
    private bool _isDisposed;

    // UI screens
    private MainMenuPane? _mainMenuPane;
    private CreateUserDialogPane? _createUserDialog;
    private DarkAges.Library.UI.ItemShop.LoginDialogPane? _loginDialog;
    private ConnectionManagerPane? _connectionManager;
    private UI.PasswordChangeDialogPane? _passwordDialog;
    private UI.CreditsPane? _creditsPane;

    public bool IsRunning => _isRunning;

    public void Initialize()
    {
        if (_isInitialized) return;
        if (_isDisposed) throw new ObjectDisposedException(nameof(DarkAgesApplication));

        try
        {
            InitializeWindow();
            InitializeGraphics();
            InitializeSystems();
            InitializeUI();
                
            _isInitialized = true;
            Console.WriteLine("Dark Ages application initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize application: {ex.Message}");
            CrashHandler.Initialize();
            throw;
        }
    }

    private void InitializeWindow()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(WINDOW_WIDTH, WINDOW_HEIGHT);
        options.Title = WINDOW_TITLE;
        options.VSync = true;

        _window = Window.Create(options);
        if (_window == null) throw new InvalidOperationException("Failed to create window");
            
        _window.Load += OnWindowLoad;
        _window.Update += OnWindowUpdate;
        _window.Render += OnWindowRender;
        _window.Closing += OnWindowClosing;
        _window.Resize += OnWindowResize;
    }

    private void InitializeGraphics()
    {
        if (_window == null) throw new InvalidOperationException("Window not initialized");

        _gl = GL.GetApi(_window);
        if (_gl == null) throw new InvalidOperationException("Failed to get OpenGL API");

        _graphicsDevice = GraphicsDevice.Instance;
        _graphicsDevice.Initialize(_window);
            
        _spriteBatch = new SpriteBatch(_gl, WINDOW_WIDTH, WINDOW_HEIGHT);

        // Initialize graphics managers
        FontManager.Initialize();
        PaletteManager.Initialize();
        ColoringTableManager.Initialize();
    }

    private void InitializeSystems()
    {
        // Initialize core systems
        _ = DarkAges.Library.Core.Events.EventManager.Instance;
        _ = DarkAges.Library.Input.InputManager.Instance;
        _ = DarkAges.Library.Network.NetworkManager.Instance;
        _ = DarkAges.Library.IO.ResourceManager.Instance;
        DarkAges.Library.Audio.SoundManager.Instance.Initialize();
    }

    private void InitializeUI()
    {
        if (_graphicsDevice == null) throw new InvalidOperationException("Graphics device not initialized");

        _mainMenuPane = new MainMenuPane();
        _createUserDialog = new CreateUserDialogPane();
        _loginDialog = new DarkAges.Library.UI.ItemShop.LoginDialogPane();
        _connectionManager = new ConnectionManagerPane();
        _passwordDialog = new UI.PasswordChangeDialogPane();
        _creditsPane = new UI.CreditsPane();

        // Add screens to manager
        var screenManager = ScreenManager.Instance;
        screenManager.AddScreen(_mainMenuPane);
        screenManager.AddScreen(_createUserDialog);
        screenManager.AddScreen(_loginDialog);
        screenManager.AddScreen(_connectionManager);
        screenManager.AddScreen(_passwordDialog);
        screenManager.AddScreen(_creditsPane);

        // Set initial screen
        screenManager.SetCurrentScreen(_mainMenuPane);
    }

    public void Run()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Application not initialized");
            
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(DarkAgesApplication));
            
        if (_window == null)
            throw new InvalidOperationException("Window not initialized");

        _isRunning = true;
        _window.Run();
    }

    public void Stop()
    {
        _isRunning = false;
        _window?.Close();
    }

    public void Shutdown()
    {
        Stop();
        Dispose();
    }

    public void ShowCreateUserDialog()
    {
        if (_createUserDialog == null) return;
        var screenManager = ScreenManager.Instance;
        screenManager.ShowModal(_createUserDialog);
    }

    public void ShowLoginDialog()
    {
        if (_loginDialog == null) return;
        var screenManager = ScreenManager.Instance;
        screenManager.ShowModal(_loginDialog);
    }

    public void ShowPasswordDialog()
    {
        if (_passwordDialog == null) return;
        var screenManager = ScreenManager.Instance;
        screenManager.ShowModal(_passwordDialog);
    }

    public void ShowCreditPane()
    {
        if (_creditsPane == null) return;
        var screenManager = ScreenManager.Instance;
        screenManager.ShowModal(_creditsPane);
    }

    public void OpenHomepage()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://darkages.com",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open homepage: {ex.Message}");
        }
    }

    public void Exit()
    {
        Shutdown();
    }

    private void OnWindowLoad()
    {
        // Window loaded event handling
    }

    private void OnWindowUpdate(double deltaTime)
    {
        if (!_isRunning) return;

        // Update game state
        EventManager.Instance.ProcessEvents();
        InputManager.Instance.ProcessInput();
    }

    private void OnWindowRender(double deltaTime)
    {
        if (!_isRunning || _gl == null || _spriteBatch == null) return;

        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _spriteBatch.Begin();

        // Render current screen
        ScreenManager.Instance.Render(_spriteBatch);

        _spriteBatch.End();
    }

    private void OnWindowClosing()
    {
        Stop();
    }

    private void OnWindowResize(Vector2D<int> size)
    {
        if (_gl == null) return;
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isRunning = false;
        
        // Dispose UI
        _mainMenuPane?.Dispose();
        _createUserDialog?.Dispose();
        _loginDialog?.Dispose();
        _connectionManager?.Dispose();
        _passwordDialog?.Dispose();
        _creditsPane?.Dispose();

        // Dispose systems
        EventManager.Instance.Clear();
        NetworkManager.Instance.Dispose();
        ResourceManager.Instance.ClearResources();
        SoundManager.Instance.Cleanup();

        // Dispose graphics
        _spriteBatch?.Dispose();
        _graphicsDevice?.Dispose();
        FontManager.Dispose();
        PaletteManager.Clear();
        ColoringTableManager.Dispose();

        // Dispose window
        _window?.Dispose();

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
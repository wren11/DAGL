using System;
using DarkAges.Library.GameLogic;
using DarkAges.Library.Network;
using DarkAges.Library.World;
using DarkAges.Library.GameLogic.Commands;
using DarkAges.Library.Core;
using DarkAges.Library.Audio;
using DarkAges.Library.Graphics;
using System.Collections.Generic;
using DarkAges.Library.UI;
using DarkAges.Library.UI.Dialogs;
using DarkAges.Library.UI.Connection;
using EventManager = DarkAges.Library.Core.Events.EventManager;

namespace DarkAges.Bot.Core;

public class GameManager : IApplication
{
    private static readonly GameManager instance = new GameManager();
    public static GameManager Instance => instance;

    public WorldObject_Human? CurrentUser { get; private set; }
    public WorldManager? World { get; private set; }
    private NetworkManager _networkManager = null!;
    private MessageHandler _messageHandler = null!;
    private CommandDispatcher _commandDispatcher = null!;
    private EventManager _eventManager = null!;
    private TimerEventMan _timerManager = null!;
    private SoundManager _soundManager = null!;
    private GraphicsDevice _graphicsDevice = null!;

    private bool _isRunning;
    public bool IsRunning => _isRunning;

    private GameManager() { }

    public void Initialize()
    {
        _networkManager = new NetworkManager();
        _eventManager = EventManager.Instance;
        _timerManager = new TimerEventMan();
        _soundManager = SoundManager.Instance;
        _graphicsDevice = new GraphicsDevice();
            
        World = new WorldManager(_networkManager);
        _messageHandler = new MessageHandler(this);
        _commandDispatcher = new CommandDispatcher(CreateCommandContext());
    }

    public void Run()
    {
        _isRunning = true;
        while (_isRunning)
        {
            _timerManager.Update(16);
            // In a real game, you would also update and render graphics here.
            System.Threading.Thread.Sleep(16); // Simulate a game loop tick
        }
    }

    public void Initialize(
        NetworkManager networkManager, 
        TimerEventMan timerManager, 
        SoundManager soundManager, 
        GraphicsDevice graphicsDevice)
    {
        _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
        _timerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager));
        _soundManager = soundManager ?? throw new ArgumentNullException(nameof(soundManager));
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            
        World = new WorldManager(_networkManager);
        _messageHandler = new MessageHandler(this);
        _commandDispatcher = new CommandDispatcher(CreateCommandContext());
    }

    public void SetUser(WorldObject_Human user)
    {
        CurrentUser = user;
    }

    private void OnAuthenticationCompleted(AuthenticationResult authResult)
    {
        if (authResult.Success)
        {
            // For now, we'll just create a new character and set it as the current user.
            // In a real implementation, you would receive character data from the server.
            var character = new WorldObject_Human { Name = "Player" };
            SetUser(character);
            World?.AddObject(character);
        }
        else
        {
            // Handle failed login
            ShowLoginDialog();
        }
    }

    private void OnPacketReceived(NetworkPacket packet)
    {
        // Here we would deserialize the packet into a message and handle it
        // For now, we'll just log it
        Console.WriteLine($"Received packet of type: {packet.Type}");
            
        // For now, we assume the packet.Data is a JSON string.
        // In a real implementation, you would have a proper serialization/deserialization mechanism.
        if (packet.Data != null)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(packet.Data);
            _messageHandler?.HandleMessage(json);
        }
    }

    private void Update(float deltaTime)
    {
        if (IsRunning)
        {
            World?.Update(deltaTime);
        }
    }

    private void OnDisconnected()
    {
        CurrentUser = null;
        World?.Clear();
        ShowLoginDialog();
    }

    public void Shutdown()
    {
        _networkManager?.Disconnect();
        _isRunning = false;
    }

    public void ShowCreateUserDialog()
    {
        var createUserDialog = new CreateUserDialogPane(); // Assuming null for parent window for now
        createUserDialog.Show();
    }

    public void ShowLoginDialog()
    {
        var loginDialog = new ConnectionManagerPane(); // Assuming null for parent window for now
        loginDialog.Show();
    }

    public void ShowPasswordDialog()
    {
        var passwordDialog = new PasswordChangeDialogPane(); // Assuming null for parent window for now
        passwordDialog.Show();
    }

    public void ShowCreditPane()
    {
        var creditPane = new CreditsPane();
        creditPane.Show();
    }

    public void OpenHomepage()
    {
        // This would open a URL in the default browser.
        // For now, we'll just log it.
        Console.WriteLine("Opening homepage...");
    }

    private CommandContext CreateCommandContext()
    {
        return new CommandContext(
            CurrentUser,
            "limbo",
            new Dictionary<string, List<WorldObject>>(),
            new Dictionary<string, Tile[,]>(),
            _eventManager,
            _soundManager,
            _graphicsDevice,
            _timerManager
        );
    }

    public void Exit()
    {
        Shutdown();
    }
}
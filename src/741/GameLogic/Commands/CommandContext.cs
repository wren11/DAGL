using DarkAges.Library.World;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Audio;
using DarkAges.Library.Graphics;
using System.Collections.Generic;
using DarkAges.Library.Core;

namespace DarkAges.Library.GameLogic.Commands;

public class CommandContext(
    WorldObject_Human? currentPlayer,
    string currentMap,
    Dictionary<string, List<WorldObject>> mapObjects,
    Dictionary<string, Tile[,]> mapTiles,
    EventManager eventManager,
    SoundManager soundManager,
    GraphicsDevice graphicsDevice,
    TimerEventMan timerManager)
{
    public WorldObject_Human? CurrentPlayer { get; set; } = currentPlayer;
    public string CurrentMap { get; set; } = currentMap;
    public Dictionary<string, List<WorldObject>> MapObjects { get; } = mapObjects;
    public Dictionary<string, Tile[,]> MapTiles { get; } = mapTiles;
    public EventManager EventManager { get; } = eventManager;
    public SoundManager SoundManager { get; } = soundManager;
    public GraphicsDevice GraphicsDevice { get; } = graphicsDevice;
    public TimerEventMan TimerManager { get; } = timerManager;
}
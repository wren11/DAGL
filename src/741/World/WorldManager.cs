using System;
using System.Collections.Generic;
using DarkAges.Library.Network;

namespace DarkAges.Library.World;

public class WorldManager(NetworkManager networkManager)
{
    private readonly NetworkManager _networkManager = networkManager;
    private readonly Dictionary<int, WorldObject> _worldObjects = new Dictionary<int, WorldObject>();

    public void Update(float deltaTime)
    {
        foreach (var obj in _worldObjects.Values)
        {
            obj.Update(deltaTime);
        }
    }

    public void AddObject(WorldObject obj)
    {
        if (!_worldObjects.ContainsKey(obj.ID))
        {
            _worldObjects.Add(obj.ID, obj);
        }
    }

    public void RemoveObject(int objectId)
    {
        _worldObjects.Remove(objectId);
    }

    public WorldObject? GetObject(int objectId)
    {
        _worldObjects.TryGetValue(objectId, out var obj);
        return obj;
    }

    public void Clear()
    {
        _worldObjects.Clear();
    }
}
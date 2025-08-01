using System;
using System.Collections.Generic;
using System.Text.Json;
using DarkAges.Bot.Core;
using DarkAges.Library.World;

namespace DarkAges.Library.Network;

public class MessageHandler
{
    private readonly GameManager _gameManager;
    private readonly Dictionary<string, Action<JsonElement>> _messageHandlers;

    public MessageHandler(GameManager gameManager)
    {
        _gameManager = gameManager;
        _messageHandlers = new Dictionary<string, Action<JsonElement>>
        {
            { "LoginResponse", HandleLoginResponse },
            { "CharacterData", HandleCharacterData },
            { "WorldUpdate", HandleWorldUpdate },
            { "PlayerStatsUpdate", HandlePlayerStatsUpdate },
            { "InventoryUpdate", HandleInventoryUpdate },
            { "ObjectSpawn", HandleObjectSpawn },
            { "ObjectDespawn", HandleObjectDespawn },
            { "NpcDialog", HandleNpcDialog },
            // Add handlers for other messages here
        };
    }

    public void HandleMessage(string messageJson)
    {
        try
        {
            var jsonDocument = JsonDocument.Parse(messageJson);
            var root = jsonDocument.RootElement;
            var messageType = root.GetProperty("MessageType").GetString();

            if (messageType != null && _messageHandlers.TryGetValue(messageType, out var handler))
            {
                handler(root);
            }
            else
            {
                Console.WriteLine($"No handler for message type: {messageType}");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing message: {ex.Message}");
        }
    }

    private void HandleLoginResponse(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<LoginResponseMessage>(data.GetRawText());
        if (message.Success)
        {
            var character = new WorldObject_Human { Name = "Player" };
            _gameManager.SetUser(character);
            _gameManager.World?.AddObject(character);
        }
        else
        {
            _gameManager.ShowLoginDialog();
        }
    }

    private void HandleCharacterData(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<CharacterDataMessage>(data.GetRawText());
        var user = _gameManager.CurrentUser;
        if (user != null && message != null)
        {
            user.Name = message.CharacterName;
            user.Level = message.Level;
            user.Experience = message.Experience;
            user.Health = message.Health;
            user.MaxHealth = message.MaxHealth;
            user.Mana = message.Mana;
            user.MaxMana = message.MaxMana;
            user.Position = new(message.PositionX, message.PositionY);
            user.CurrentMap = message.CurrentMap;
            user.Class = message.Class;
        }
    }

    private void HandleWorldUpdate(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<WorldUpdateMessage>(data.GetRawText());
        if (message?.UpdateData == null) return;

        var updateData = JsonDocument.Parse(message.UpdateData.ToString()).RootElement;

        switch (message.UpdateType)
        {
            case "ObjectPosition":
                var objectId = updateData.GetProperty("ObjectId").GetInt32();
                var x = updateData.GetProperty("X").GetSingle();
                var y = updateData.GetProperty("Y").GetSingle();
                var worldObject = _gameManager.World?.GetObject(objectId);
                if (worldObject != null)
                {
                    worldObject.Position = new(x, y);
                }
                break;
            // Add other world update types here
        }
    }

    private void HandlePlayerStatsUpdate(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<PlayerStatsUpdateMessage>(data.GetRawText());
        var user = _gameManager.CurrentUser;
        if (user != null && message != null)
        {
            user.Health = message.Health;
            user.MaxHealth = message.MaxHealth;
            user.Mana = message.Mana;
            user.MaxMana = message.MaxMana;
            user.Experience = message.Experience;
            user.Level = message.Level;
        }
    }

    private void HandleInventoryUpdate(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<InventoryUpdateMessage>(data.GetRawText());
        var user = _gameManager.CurrentUser;
        if (user != null && message != null)
        {
            user.Inventory.Clear();
            foreach (var item in message.Items)
            {
                var newItem = new GameLogic.Item
                {
                    ItemId = item.ItemId,
                    Quantity = item.Quantity
                };
                user.AddItem(newItem);
            }
        }
    }

    private void HandleObjectSpawn(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<ObjectSpawnMessage>(data.GetRawText());
        if (message == null) return;

        World.WorldObject newObject = message.ObjectType switch
        {
            "Player" => new World.WorldObject_Human { Name = "OtherPlayer" },
            "NPC" => new World.WorldObject_NPC(),
            "Monster" => new World.WorldObject_NPC(), // Monsters are treated as NPCs
            "Item" => new World.WorldObject_Item(new GameLogic.Item()),
            _ => null
        };

        if (newObject != null)
        {
            newObject.Position = new(message.PositionX, message.PositionY);
            _gameManager.World?.AddObject(newObject);
        }
    }

    private void HandleObjectDespawn(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<ObjectDespawnMessage>(data.GetRawText());
        if (message == null) return;
        _gameManager.World?.RemoveObject(message.ObjectId);
    }
        
    private void HandleNpcDialog(JsonElement data)
    {
        var message = JsonSerializer.Deserialize<NpcDialogMessage>(data.GetRawText());
        if (message == null) return;

        // TODO: Create appropriate NPC dialog - NpcDialog class doesn't exist yet
        // var dialog = new UI.Dialogs.NpcDialog(null, message.DialogText, message.Options);
        // dialog.Show();
        Console.WriteLine($"NPC Dialog: {message.DialogText}");
    }
}
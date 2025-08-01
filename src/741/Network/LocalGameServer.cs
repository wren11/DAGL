using System.Collections.Concurrent;
using DarkAges.Library.World;

namespace DarkAges.Library.Network;

/// <summary>
/// Local game server for offline mode
/// </summary>
public class LocalGameServer(NetworkManager networkManager) : IDisposable
{
    private readonly Dictionary<int, WorldObject_Human> _players = new();
    private readonly Dictionary<string, List<WorldObject>> _mapObjects = new();
    private bool _isRunning;
    private Timer _updateTimer;
    private readonly PacketProcessor _packetProcessor = new();
    private readonly ConcurrentQueue<NetworkPacket> _incomingPackets = new();
    private readonly ConcurrentQueue<NetworkPacket> _outgoingPackets = new();

    public async Task StartAsync()
    {
        if (_isRunning) return;

        _isRunning = true;
        _updateTimer = new Timer(Update, null, TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16)); // 60 FPS
            
        await Task.CompletedTask;
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _updateTimer?.Dispose();
    }

    public void HandleClientPacket(byte[] data)
    {
        try
        {
            var packet = NetworkPacket.Deserialize(data);
            ProcessClientPacket(packet);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing client packet: {ex.Message}");
        }
    }

    private void ProcessClientPacket(NetworkPacket packet)
    {
        switch (packet.Type)
        {
        case PacketType.Login:
            HandleLogin(packet);
            break;
        case PacketType.CharacterMove:
            HandleCharacterMove(packet);
            break;
        case PacketType.ChatMessage:
            HandleChatMessage(packet);
            break;
        case PacketType.UseSkill:
            HandleUseSkill(packet);
            break;
        case PacketType.CastSpell:
            HandleCastSpell(packet);
            break;
        case PacketType.UseItem:
            HandleUseItem(packet);
            break;
        default:
            Console.WriteLine($"Unhandled packet type: {packet.Type}");
            break;
        }
    }

    private void HandleLogin(NetworkPacket packet)
    {
        // Simulate successful login
        var response = new NetworkPacket
        {
            Type = PacketType.LoginResponse,
            Data = new Dictionary<string, object>
            {
                ["success"] = true,
                ["playerId"] = 1,
                ["message"] = "Welcome to Dark Ages!"
            }
        };
        SendToClient(response);
    }

    private void HandleCharacterMove(NetworkPacket packet)
    {
        // Process movement
        if (packet.Data.TryGetValue("x", out var x) && packet.Data.TryGetValue("y", out var y))
        {
            // Update player position
            Console.WriteLine($"Player moved to ({x}, {y})");
        }
    }

    private void HandleChatMessage(NetworkPacket packet)
    {
        if (packet.Data.TryGetValue("message", out var message))
        {
            Console.WriteLine($"Chat: {message}");
                
            // Echo back to client
            var response = new NetworkPacket
            {
                Type = PacketType.ChatMessage,
                Data = new Dictionary<string, object>
                {
                    ["sender"] = "System",
                    ["message"] = $"Echo: {message}"
                }
            };
            SendToClient(response);
        }
    }

    private void HandleUseSkill(NetworkPacket packet)
    {
        if (packet.Data.TryGetValue("skillId", out var skillId))
        {
            Console.WriteLine($"Player used skill {skillId}");
        }
    }

    private void HandleCastSpell(NetworkPacket packet)
    {
        if (packet.Data.TryGetValue("spellId", out var spellId))
        {
            Console.WriteLine($"Player cast spell {spellId}");
        }
    }

    private void HandleUseItem(NetworkPacket packet)
    {
        if (packet.Data.TryGetValue("itemId", out var itemId))
        {
            Console.WriteLine($"Player used item {itemId}");
        }
    }

    private void SendToClient(NetworkPacket packet)
    {
        try
        {
            var data = packet.Serialize();
            networkManager?.OnDataReceived(this, new SocketDataEventArgs(data));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending to client: {ex.Message}");
        }
    }

    private void Update(object? state)
    {
        if (!_isRunning) return;

        // Update game logic
        UpdateWorldObjects();
        ProcessAI();
        CheckCollisions();
    }

    private void UpdateWorldObjects()
    {
        foreach (var mapObjects in _mapObjects.Values)
        {
            foreach (var obj in mapObjects.ToList())
            {
                obj.Update(0.016f); // 60 FPS
            }
        }
    }

    private void ProcessAI()
    {
        // Process AI for NPCs and monsters
    }

    private void CheckCollisions()
    {
        // Check collisions between objects
    }

    public bool HasMessages()
    {
        return !_incomingPackets.IsEmpty;
    }

    public object? GetNextMessage()
    {
        if (_incomingPackets.TryDequeue(out var packet))
        {
            return ConvertPacketToMessage(packet);
        }
        return null;
    }

    public void SendCreateAccountRequest(string username, string hashedPassword)
    {
        var message = new CreateAccountRequestMessage
        {
            Username = username,
            HashedPassword = hashedPassword,
            MessageId = GenerateMessageId()
        };

        SendMessage(message);
    }

    private object ConvertPacketToMessage(NetworkPacket packet)
    {
        return packet.Type switch
        {
            PacketType.LoginResponse => new LoginResponseMessage
            {
                Success = packet.Data.Count > 0 && (bool)packet.Data["success"],
                ErrorMessage = packet.Data.Count > 1 ? (string)packet.Data["errorMessage"] : null
            },
            PacketType.CharacterData => new CharacterDataMessage
            {
                CharacterName = (string)packet.Data["characterName"]
            },
            PacketType.ObjectUpdate => new WorldUpdateMessage
            {
                UpdateType = "ObjectUpdate",
                UpdateData = packet.Data
            },
            _ => packet
        };
    }

    private void SendMessage(NetworkMessage message)
    {
        try
        {
            var packet = new NetworkPacket
            {
                Type = GetPacketTypeFromMessage(message)
            };

            _outgoingPackets.Enqueue(packet);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    private PacketType GetPacketTypeFromMessage(NetworkMessage message)
    {
        return message.MessageType switch
        {
            "LoginRequest" => PacketType.LoginRequest,
            "CreateAccountRequest" => PacketType.LoginRequest, // Reuse login packet type for account creation
            "CharacterData" => PacketType.CharacterData,
            "PlayerMove" => PacketType.CharacterMove,
            "Chat" => PacketType.ChatMessage,
            "ItemAction" => PacketType.UseItem,
            "SpellCast" => PacketType.CastSpell,
            "SkillUse" => PacketType.UseSkill,
            "Disconnect" => PacketType.Disconnect,
            _ => PacketType.LoginRequest
        };
    }

    private int GenerateMessageId()
    {
        return Environment.TickCount;
    }

    public void Dispose()
    {
        Stop();
    }
}
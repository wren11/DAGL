using System;
using System.Collections.Generic;

namespace DarkAges.Library.Network;

public class PacketHandler
{
    private readonly Dictionary<PacketType, List<IPacketHandler>> _handlers = new();

    public void Register(PacketType type, IPacketHandler handler)
    {
        if (!_handlers.TryGetValue(type, out var list))
        {
            list = [];
            _handlers[type] = list;
        }
        if (!list.Contains(handler))
            list.Add(handler);
    }

    public void Deregister(PacketType type, IPacketHandler handler)
    {
        if (_handlers.TryGetValue(type, out var list))
        {
            list.Remove(handler);
            if (list.Count == 0)
                _handlers.Remove(type);
        }
    }

    public void HandleIncomingPacket(byte[] packet)
    {
        var type = PacketParser.GetPacketType(packet);
        if (_handlers.TryGetValue(type, out var list))
        {
            foreach (var handler in list)
            {
                handler.HandlePacket(packet);
            }
        }
    }
}

public class PacketParser
{
    public static PacketType GetPacketType(byte[] packet)
    {
        // Assuming the first byte of the packet indicates the type
        if (packet.Length > 0)
        {
            return (PacketType)packet[0];
        }
        throw new ArgumentException("Invalid packet data");
    }
}
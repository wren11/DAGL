namespace DarkAges.Library.Network;

/// <summary>
/// Packet processor for handling network messages
/// </summary>
public class PacketProcessor
{
    public void ProcessIncomingPacket(NetworkPacket packet)
    {
        switch (packet.Type)
        {
        case PacketType.LoginResponse:
            ProcessLoginResponse(packet);
            break;
        case PacketType.CharacterData:
            ProcessCharacterData(packet);
            break;
        case PacketType.MapData:
            ProcessMapData(packet);
            break;
        case PacketType.ChatMessage:
            ProcessChatMessage(packet);
            break;
        case PacketType.ObjectUpdate:
            ProcessObjectUpdate(packet);
            break;
        default:
            Console.WriteLine($"Unhandled incoming packet: {packet.Type}");
            break;
        }
    }

    private void ProcessLoginResponse(NetworkPacket packet)
    {
        if (packet.Data.TryGetValue("success", out var success) && (bool)success)
        {
            Console.WriteLine("Login successful!");
        }
        else
        {
            Console.WriteLine("Login failed!");
        }
    }

    private void ProcessCharacterData(NetworkPacket packet)
    {
        Console.WriteLine("Received character data");
    }

    private void ProcessMapData(NetworkPacket packet)
    {
        Console.WriteLine("Received map data");
    }

    private void ProcessChatMessage(NetworkPacket packet)
    {
        if (packet.Data.TryGetValue("message", out var message))
        {
            Console.WriteLine($"Chat received: {message}");
        }
    }

    private void ProcessObjectUpdate(NetworkPacket packet)
    {
        Console.WriteLine("Received object update");
    }
}
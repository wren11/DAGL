using System.Text;

namespace DarkAges.Library.Network;

/// <summary>
/// Network packet structure
/// </summary>
public class NetworkPacket
{
    public PacketType Type { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int Length => Serialize().Length;

    public byte[] Serialize()
    {
        // Simple serialization - in a real implementation this would use a proper protocol
        var typeBytes = BitConverter.GetBytes((int)Type);
        var dataJson = System.Text.Json.JsonSerializer.Serialize(Data);
        var dataBytes = Encoding.UTF8.GetBytes(dataJson);
        var lengthBytes = BitConverter.GetBytes(dataBytes.Length);

        var result = new byte[4 + 4 + dataBytes.Length];
        typeBytes.CopyTo(result, 0);
        lengthBytes.CopyTo(result, 4);
        dataBytes.CopyTo(result, 8);

        return result;
    }

    public static NetworkPacket Deserialize(byte[] data)
    {
        if (data.Length < 8) throw new ArgumentException("Invalid packet data");

        var type = (PacketType)BitConverter.ToInt32(data, 0);
        var length = BitConverter.ToInt32(data, 4);

        if (data.Length < 8 + length) throw new ArgumentException("Incomplete packet data");

        var dataJson = Encoding.UTF8.GetString(data, 8, length);
        var packetData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(dataJson) ?? new();

        return new NetworkPacket
        {
            Type = type,
            Data = packetData
        };
    }
}
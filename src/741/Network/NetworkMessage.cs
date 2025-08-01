using System;

namespace DarkAges.Library.Network;

public abstract class NetworkMessage
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int MessageId { get; set; }
    public string MessageType { get; set; } = "";
}
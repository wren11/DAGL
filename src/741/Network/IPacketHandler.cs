namespace DarkAges.Library.Network;

public interface IPacketHandler
{
    void HandlePacket(byte[] packet);
}
namespace DarkAges.Library.UI.ItemShop;

public class NetworkManager
{
    private static NetworkManager _instance;
    public static NetworkManager Instance => _instance ??= new NetworkManager();

    public void SendPacket(byte[] packet)
    {
        // Real implementation would send the packet over the network
        System.Console.WriteLine($"Sending packet: {BitConverter.ToString(packet)}");
    }
}
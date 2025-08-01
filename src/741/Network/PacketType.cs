namespace DarkAges.Library.Network;

/// <summary>
/// Packet types
/// </summary>
public enum PacketType
{
    Login = 1,
    LoginRequest = 1,
    LoginResponse = 2,
    CharacterData = 3,
    MapData = 4,
    ChatMessage = 5,
    CharacterMove = 6,
    ObjectUpdate = 7,
    UseSkill = 8,
    CastSpell = 9,
    UseItem = 10,
    Disconnect = 99
}
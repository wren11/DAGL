namespace DarkAges.Library.Core.Events;

public class GameEvent<T>(T data) : Event(EventType.None)
{
    public T Data { get; } = data;
}
namespace DarkAges.Library.Core.Events;

public interface IEventHandler
{
    bool HandleEvent(Event ev);
}
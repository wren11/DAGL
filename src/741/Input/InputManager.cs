using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using Silk.NET.Input;
using Keys = Silk.NET.Input.Key;

namespace DarkAges.Library.Input;

public class InputManager
{
    private static InputManager? _instance;
    public static InputManager Instance => _instance ??= new InputManager();

    private readonly List<Event> _inputQueue = [];
    private readonly Dictionary<Keys, bool> _keyStates = new Dictionary<Keys, bool>();
    private readonly Dictionary<Core.Events.MouseButton, bool> _mouseStates = new Dictionary<Core.Events.MouseButton, bool>();

    private InputManager()
    {
    }

    public void ProcessInput()
    {
        // Process input events
        foreach (var inputEvent in _inputQueue)
        {
            HandleInputEvent(inputEvent);
        }
        _inputQueue.Clear();
    }

    public void QueueInput(Event inputEvent)
    {
        _inputQueue.Add(inputEvent);
    }

    public bool IsKeyPressed(Keys key)
    {
        return _keyStates.TryGetValue(key, out var pressed) && pressed;
    }

    public bool IsMouseButtonPressed(Core.Events.MouseButton button)
    {
        return _mouseStates.TryGetValue(button, out var pressed) && pressed;
    }

    private void HandleInputEvent(Event inputEvent)
    {
        switch (inputEvent)
        {
        case KeyEvent keyEvent:
            _keyStates[keyEvent.Key] = keyEvent.Type == EventType.KeyDown;
            break;
        case MouseEvent mouseEvent:
            _mouseStates[mouseEvent.Button] = mouseEvent.Type == EventType.MouseDown;
            break;
        }
    }
}

// InputEvent is now defined in DarkAges.Library.Core.Events
// This class is no longer needed
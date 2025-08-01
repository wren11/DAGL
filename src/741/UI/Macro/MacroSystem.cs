using System;
using System.Collections.Generic;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.Macro;

public class MacroSystem : ControlPane
{
    private MacroDialog _macroDialog;
    private readonly List<MacroDefinition> _macros = [];

    public event EventHandler<MacroDefinition> MacroExecuted;

    public MacroSystem()
    {
        _macroDialog = new MacroDialog();
        _macroDialog.MacroCreated += (s, macro) => AddMacro(macro);
        _macroDialog.MacroEdited += (s, macro) => UpdateMacro(macro);
        _macroDialog.MacroDeleted += (s, macro) => RemoveMacro(macro);
    }

    public void ShowMacroDialog()
    {
        _macroDialog.Show();
    }

    public void AddMacro(MacroDefinition macro)
    {
        _macros.Add(macro);
    }

    public void UpdateMacro(MacroDefinition macro)
    {
        var index = _macros.FindIndex(m => m.Id == macro.Id);
        if (index >= 0)
        {
            _macros[index] = macro;
        }
    }

    public void RemoveMacro(MacroDefinition macro)
    {
        _macros.RemoveAll(m => m.Id == macro.Id);
    }

    public void ExecuteMacro(int macroId)
    {
        var macro = _macros.Find(m => m.Id == macroId);
        if (macro != null)
        {
            ExecuteMacro(macro);
        }
    }

    public void ExecuteMacro(MacroDefinition macro)
    {
        foreach (var action in macro.Actions)
        {
            ExecuteMacroAction(action);
        }
        MacroExecuted?.Invoke(this, macro);
    }

    private void ExecuteMacroAction(MacroAction action)
    {
        switch (action.Type)
        {
        case MacroActionType.KeyPress:
            HandleKeyPress(action.Parameters);
            break;
        case MacroActionType.MouseClick:
            HandleMouseClick(action.Parameters);
            break;
        case MacroActionType.Delay:
            HandleDelay(action.Parameters);
            break;
        case MacroActionType.ChatCommand:
            HandleChatCommand(action.Parameters);
            break;
        case MacroActionType.ItemUse:
            HandleItemUse(action.Parameters);
            break;
        case MacroActionType.SpellCast:
            HandleSpellCast(action.Parameters);
            break;
        }
    }

    private void HandleKeyPress(string parameters)
    {
        if (int.TryParse(parameters, out var keyCode))
        {
            // Create a proper key event with correct parameters
            var keyEvent = new KeyEvent(EventType.KeyDown, (Silk.NET.Input.Key)keyCode);
            EventManager.Instance.PostEvent(keyEvent);
        }
    }

    private void HandleMouseClick(string parameters)
    {
        var parts = parameters.Split(',');
        if (parts.Length >= 2 && int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y))
        {
            // Create a proper mouse event with correct parameters
            var mouseEvent = new MouseEvent(EventType.MouseDown, x, y, Core.Events.MouseButton.Left);
            EventManager.Instance.PostEvent(mouseEvent);
        }
    }

    private void HandleDelay(string parameters)
    {
        if (int.TryParse(parameters, out var delayMs))
        {
            System.Threading.Thread.Sleep(delayMs);
        }
    }

    private void HandleChatCommand(string parameters)
    {
        // Create a custom chat event
        var chatEvent = new ChatEvent(parameters, ChatType.Command);
        // For now, just log the chat command since EventManager might not handle custom events
        Console.WriteLine($"Chat command: {parameters}");
    }

    private void HandleItemUse(string parameters)
    {
        if (int.TryParse(parameters, out var itemId))
        {
            // Create a custom item event
            var itemEvent = new ItemEvent(itemId, ItemEventType.Use);
            // For now, just log the item use since EventManager might not handle custom events
            Console.WriteLine($"Item use: {itemId}");
        }
    }

    private void HandleSpellCast(string parameters)
    {
        if (int.TryParse(parameters, out var spellId))
        {
            // Create a custom spell event
            var spellEvent = new SpellEvent(spellId, SpellEventType.Cast);
            // For now, just log the spell cast since EventManager might not handle custom events
            Console.WriteLine($"Spell cast: {spellId}");
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        _macroDialog.Render(spriteBatch);
        base.Render(spriteBatch);
    }

    public override bool HandleEvent(Event e)
    {
        if (!IsVisible) return false;

        if (_macroDialog.HandleEvent(e)) return true;

        return base.HandleEvent(e);
    }

    public override void Dispose()
    {
        _macroDialog?.Dispose();
    }
}
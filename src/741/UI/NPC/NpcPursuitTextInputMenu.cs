using System;
using System.Collections.Generic;
using System.Text;

namespace DarkAges.Library.UI.NPC;

public class NpcPursuitTextInputMenu : NPCTextInputMenu
{
    // Corresponds to *(this + 8) = 0, *(this + 264) = 0, *(this + 265) = 0
    public byte PursuitFlag1 { get; set; } = 0;
    public byte PursuitFlag2 { get; set; } = 0;
    public byte PursuitFlag3 { get; set; } = 0;
    public PursuitMessage CurrentMessage { get; private set; }

    // from sub_53DFA0

    public void Initialize(PursuitMessage message)
    {
        // Corresponds to sub_53E1D0
        CurrentMessage = message;

        // Here we would parse the message and set properties of the menu
        // such as the prompt text, default value, etc.
        // For now, this is a placeholder.
    }
}
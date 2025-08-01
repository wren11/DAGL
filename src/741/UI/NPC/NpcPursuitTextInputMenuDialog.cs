using System;
using System.Collections.Generic;
using System.Drawing;
using DarkAges.Library.Core.Events;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI.NPC;

public class NpcPursuitTextInputMenuDialog : NPCTextInputMenuDialog
{
    private NpcPursuitTextInputMenu _pursuitInputMenu = new();

    // The logic from sub_53E5A0 would go here, which sets up the dialog
    // with the pursuit-specific text input menu.

    public void Initialize(PursuitMessage message)
    {
        _pursuitInputMenu.Initialize(message);
    }
}
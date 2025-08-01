using DarkAges.Library.UI;
using System;
using System.Collections.Generic;

namespace DarkAges.Library.UI.NPC;

public class NpcPursuitMessageDialog : DialogPane
{
    private ButtonControlPane _button1 = new();
    private ButtonControlPane _button2 = new();
    private ButtonControlPane _button3 = new();
        
    public PursuitMessage CurrentMessage { get; private set; }

    // Constructor logic from sub_53CC10
    // The base constructor from DialogPane should be sufficient for now.
    // from sub_445670 calls
    // We don't have AddChild in DialogPane, I'll have to add it.
    // For now, I'll just keep the buttons as private fields.
    // from sub_445820 - setting some initial state
    // For now, let's assume this sets the focused button or a default action.

    public void Initialize(PursuitMessage message)
    {
        // This method corresponds to sub_53CE70
        CurrentMessage = message;

        // sub_52D760 - sets text
        // sub_52D7C0 - sets face image
            
        // The switch statement in sub_53CE70 handles different message types.
        // We can represent this with a method dispatch on the message type.
            
        HandleMessageType();
    }

    private void HandleMessageType()
    {
        if (CurrentMessage == null) return;

        switch (CurrentMessage.MessageType)
        {
        case 2: // NPC_Pursuit_MenuQuestionMessage
            // logic from case 2 in sub_53CE70
            break;
        case 3: // NPC_Pursuit_SimpleMenuQuestionMessage
            // logic from case 3 in sub_53CE70
            break;
        case 4: // NPC_Pursuit_TextMessage
            // logic from case 4 in sub_53CE70
            break;
        case 5: // NPC_Pursuit_SimpleTextMessage
            // logic from case 5 in sub_53CE70
            break;
        case 6: // NPC_Pursuit_QuestionMessageFace
            // logic from case 6 in sub_53CE70
            break;
        case 9: // NPC_Pursuit_NexonclubIdTextMessage
            // logic from case 9 in sub_53CE70
            break;
        case 10: // Close dialog
            Close();
            break;
        default:
            break;
        }
    }
}
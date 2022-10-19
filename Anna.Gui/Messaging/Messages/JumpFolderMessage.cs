﻿using Anna.Constants;

namespace Anna.Gui.Messaging.Messages;

public sealed class JumpFolderMessage : InteractionMessage
{
    public (DialogResultTypes DialogResult, string Path) Response { get; internal set; }

    public JumpFolderMessage(string messageKey)
        : base(messageKey)
    {
    }
}
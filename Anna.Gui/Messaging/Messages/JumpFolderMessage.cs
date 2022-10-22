using Anna.Constants;

namespace Anna.Gui.Messaging.Messages;

public sealed class JumpFolderMessage : InteractionMessage
{
    public (DialogResultTypes DialogResult, string Path) Response { get; internal set; }

    public readonly string CurrentFolderPath;

    public JumpFolderMessage(string currentFolderPath, string messageKey)
        : base(messageKey)
    {
        CurrentFolderPath = currentFolderPath;
    }
}
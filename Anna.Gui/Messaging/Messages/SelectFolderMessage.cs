using Anna.Constants;

namespace Anna.Gui.Messaging.Messages;

public sealed class SelectFolderMessage : InteractionMessage
{
    public (DialogResultTypes DialogResult, string Path) Response { get; internal set; }

    public SelectFolderMessage(string messageKey)
        : base(messageKey)
    {
    }
}
using Anna.Gui.Views.Windows.Base;

namespace Anna.Gui.Messaging;

public sealed class JumpFolderMessage : InteractionMessage
{
    public (DialogResultTypes DialogResult, string Path) Response { get; internal set; }

    public JumpFolderMessage(string messageKey)
        : base(messageKey)
    {
    }
}
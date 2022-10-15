using Anna.Gui.Views.Windows.Base;

namespace Anna.Gui.Messaging;

public sealed class SelectFolderMessage : InteractionMessage
{
    public (DialogResultTypes DialogResult, string Path) Response { get; internal set; }

    public SelectFolderMessage(string messageKey)
        : base(messageKey)
    {
    }
}
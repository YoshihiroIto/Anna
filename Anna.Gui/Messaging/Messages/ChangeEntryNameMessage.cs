using Anna.Gui.Views.Windows.Base;

namespace Anna.Gui.Messaging.Messages;

public sealed class ChangeEntryNameMessage : InteractionMessage
{
    public string CurrentFolderPath { get; init; }
    public string CurrentFilename { get; init; }

    public (DialogResultTypes DialogResult, string FilePath) Response { get; internal set; }

    public ChangeEntryNameMessage(string currentFolderPath, string currentFilename, string messageKey)
        : base(messageKey)
    {
        CurrentFolderPath = currentFolderPath;
        CurrentFilename = currentFilename;
    }
}
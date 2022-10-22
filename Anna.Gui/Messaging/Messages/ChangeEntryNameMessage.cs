using Anna.Constants;

namespace Anna.Gui.Messaging.Messages;

public sealed class ChangeEntryNameMessage : InteractionMessage
{
    public (DialogResultTypes DialogResult, string FilePath) Response { get; internal set; }

    public readonly string CurrentFolderPath;
    public readonly string CurrentFilename;

    public ChangeEntryNameMessage(string currentFolderPath, string currentFilename, string messageKey)
        : base(messageKey)
    {
        CurrentFolderPath = currentFolderPath;
        CurrentFilename = currentFilename;
    }
}
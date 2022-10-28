using Anna.Constants;

namespace Anna.Gui.Messaging.Messages;

public sealed class InputEntryNameMessage : InteractionMessage
{
    public (DialogResultTypes DialogResult, string FilePath) Response { get; internal set; }

    public readonly string CurrentFolderPath;
    public readonly string CurrentFilename;
    public readonly string Title;
    public readonly bool IsEnableCurrentInfo;
    public readonly bool IsEnableSkip;

    public InputEntryNameMessage(
        string currentFolderPath, 
        string currentFilename, 
        string title,
        bool isEnableCurrentInfo,
        bool isEnableSkip,
        string messageKey)
        : base(messageKey)
    {
        CurrentFolderPath = currentFolderPath;
        CurrentFilename = currentFilename;
        Title = title;
        IsEnableCurrentInfo = isEnableCurrentInfo;
        IsEnableSkip = isEnableSkip;
    }
}
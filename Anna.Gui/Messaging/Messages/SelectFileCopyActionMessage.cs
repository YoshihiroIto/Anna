using Anna.Constants;
using Anna.DomainModel.FileSystem.FileProcessable;

namespace Anna.Gui.Messaging.Messages;

public sealed class SelectFileCopyActionMessage : InteractionMessage
{
    public readonly string SrcPath;
    public readonly string DestPath;

    public (DialogResultTypes DialogResult, FileSystemCopier.CopyActionWhenExistsResult Result) Response
    {
        get;
        internal set;
    }

    public SelectFileCopyActionMessage(
        string srcPath,
        string destPath,
        string messageKey)
        : base(messageKey)
    {
        SrcPath = srcPath;
        DestPath = destPath;
    }
}
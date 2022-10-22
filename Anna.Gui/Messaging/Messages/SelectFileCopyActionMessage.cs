using Anna.Constants;
using Anna.DomainModel.FileSystem;

namespace Anna.Gui.Messaging.Messages;

public sealed class SelectFileCopyActionMessage : InteractionMessage
{
    public string SrcPath { get; }
    public string DestPath { get; }

    public (DialogResultTypes DialogResult, FileSystemCopier.CopyStrategyWhenExistsResult Result) Response
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
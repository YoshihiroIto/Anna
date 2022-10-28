using Anna.Constants;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Localization;
using Anna.Service;
using System.Diagnostics;
using System.IO;

namespace Anna.Gui.BackgroundOperators.Internals;

internal sealed class ConfirmedFileSystemCopier
    : FileSystemCopier
        , IHasArg<(InteractionMessenger Messenger, CopyOrMove CopyOrMove)>
{
    private readonly (InteractionMessenger Messenger, CopyOrMove CopyOrMove) _arg;
    private FastSpinLock _lockObj;

    public ConfirmedFileSystemCopier(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out _arg);

        CopyOrMove = _arg.CopyOrMove;
    }

    protected override void CopyActionWhenExists(string srcPath, string destPath, ref CopyActionWhenExistsResult result)
    {
        Debug.Assert(CancellationTokenSource is not null);
        
        try
        {
            _lockObj.Enter();
            
            if (result.IsSameActionThereafter)
                return;

            if (CancellationTokenSource.IsCancellationRequested)
                return;

            var message = _arg.Messenger.Raise(
                new SelectFileCopyActionMessage(
                    srcPath,
                    destPath,
                    result.IsFirst,
                    WindowBaseViewModel.MessageKeySelectFileCopyAction));

            result = message.Response.Result;

            if (message.Response.DialogResult == DialogResultTypes.Cancel)
                CancellationTokenSource.Cancel();
        }
        finally
        {
            _lockObj.Exit();
        }
    }

    protected override CopyActionWhenSamePathResult CopyActionWhenSamePath(string destPath)
    {
        Debug.Assert(CancellationTokenSource is not null);
        
        try
        {
            _lockObj.Enter();
            
            if (CancellationTokenSource.IsCancellationRequested)
                return new CopyActionWhenSamePathResult(SamePathCopyFileActions.Skip, "");

            var folder = Path.GetDirectoryName(destPath) ?? "";
            var fileName = Path.GetFileName(destPath);

            var message = _arg.Messenger.Raise(
                new InputEntryNameMessage(
                    folder,
                    fileName,
                    Resources.DialogTitle_ChangeEntryName,
                    true,
                    true,
                    WindowBaseViewModel.MessageKeyInputEntryName));

            if (message.Response.DialogResult == DialogResultTypes.Cancel)
                CancellationTokenSource.Cancel();

            return new CopyActionWhenSamePathResult(
                message.Response.DialogResult == DialogResultTypes.Ok
                    ? SamePathCopyFileActions.Override
                    : SamePathCopyFileActions.Skip,
                message.Response.FilePath);
        }
        finally
        {
            _lockObj.Exit();
        }
    }
}
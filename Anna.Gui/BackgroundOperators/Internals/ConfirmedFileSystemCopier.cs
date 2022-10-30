using Anna.Constants;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Gui.Views.Windows.Dialogs;
using Anna.Localization;
using Anna.Service;
using System.Diagnostics;
using System.IO;

namespace Anna.Gui.BackgroundOperators.Internals;

internal sealed class ConfirmedFileSystemCopier
    : FileSystemCopier
        , IHasArg<(Messenger Messenger, CopyOrMove CopyOrMove)>
{
    private readonly (Messenger Messenger, CopyOrMove CopyOrMove) _arg;
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

            using var viewModel = Dic.GetInstance<SelectFileCopyActionDialogViewModel, (string, string, bool)>
                ((srcPath, destPath, result.IsFirst));

            _arg.Messenger.Raise(new TransitionMessage(viewModel, WindowBaseViewModel.MessageKeySelectFileCopy));

            result = viewModel.Result;

            if (viewModel.DialogResult == DialogResultTypes.Cancel)
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

            using var viewModel =
                Dic.GetInstance<InputEntryNameDialogViewModel, (string, string, string, bool, bool)>(
                    (folder, fileName, Resources.DialogTitle_ChangeEntryName, true, true));

            _arg.Messenger.Raise(new TransitionMessage(viewModel, WindowBaseViewModel.MessageKeyInputEntryName));

            if (viewModel.DialogResult == DialogResultTypes.Cancel)
                CancellationTokenSource.Cancel();
                
            return new CopyActionWhenSamePathResult(
                viewModel.DialogResult == DialogResultTypes.Ok
                    ? SamePathCopyFileActions.Override
                    : SamePathCopyFileActions.Skip,
                viewModel.ResultFilePath);
        }
        finally
        {
            _lockObj.Exit();
        }
    }
}
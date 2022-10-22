using Anna.Constants;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Service;
using Avalonia.Threading;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Anna.Gui.BackgroundOperators.Internals;

internal sealed class ConfirmedFileSystemCopier
    : FileSystemCopier
        , IHasArg<(InteractionMessenger Messenger, int Dummy)>
{
    private readonly (InteractionMessenger Messenger, int Dummy) _arg;
    private readonly object _lockObj = new();

    public ConfirmedFileSystemCopier(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out _arg);
    }

    protected override void CopyActionWhenExists(string srcPath, string destPath, ref CopyActionWhenExistsResult result)
    {
        lock (_lockObj)
        {
            if (result.IsSameActionThereafter)
                return;

            Debug.Assert(CancellationTokenSource is not null);

            var resultDialogResult = DialogResultTypes.Cancel;
            result = new CopyActionWhenExistsResult(ExistsCopyFileActions.Skip, "", false);

            if (CancellationTokenSource.IsCancellationRequested)
                return;

            using var m = new ManualResetEventSlim();

            var tempResult = new CopyActionWhenExistsResult(ExistsCopyFileActions.Skip, "", false);

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var message = await _arg.Messenger.RaiseAsync(
                    new SelectFileCopyActionMessage(
                        srcPath,
                        destPath,
                        WindowBaseViewModel.MessageKeySelectFileCopyAction));

                resultDialogResult = resultDialogResult = message.Response.DialogResult;
                tempResult = message.Response.Result;

                // ReSharper disable once AccessToDisposedClosure
                m.Set();
            });

            m.Wait();

            result = tempResult;

            if (resultDialogResult == DialogResultTypes.Cancel)
                CancellationTokenSource.Cancel();
        }
    }

    protected override CopyActionWhenSamePathResult CopyActionWhenSamePath(
        string destPath)
    {
        lock (_lockObj)
        {
            Debug.Assert(CancellationTokenSource is not null);

            if (CancellationTokenSource.IsCancellationRequested)
                return new CopyActionWhenSamePathResult(SamePathCopyFileActions.Skip, "");

            var resultDialogResult = DialogResultTypes.Cancel;
            var resultFilePath = "";

            using var m = new ManualResetEventSlim();

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var folder = Path.GetDirectoryName(destPath) ?? "";
                var filename = Path.GetFileName(destPath);

                var message = await _arg.Messenger.RaiseAsync(
                    new ChangeEntryNameMessage(
                        folder,
                        filename,
                        WindowBaseViewModel.MessageKeyChangeEntryName));

                resultDialogResult = message.Response.DialogResult;
                resultFilePath = message.Response.FilePath;

                // ReSharper disable once AccessToDisposedClosure
                m.Set();
            });

            m.Wait();

            if (resultDialogResult == DialogResultTypes.Cancel)
                CancellationTokenSource.Cancel();

            return new CopyActionWhenSamePathResult(
                resultDialogResult == DialogResultTypes.Ok
                    ? SamePathCopyFileActions.Override
                    : SamePathCopyFileActions.Skip,
                resultFilePath);
        }
    }
}
using Anna.Constants;
using Anna.DomainModel.FileSystem;
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

    protected override CopyStrategyWhenExistsResult CopyStrategyWhenExists(string srcPath, string destPath)
    {
        lock (_lockObj)
        {
            Debug.Assert(CancellationTokenSource is not null);

            var resultDialogResult = DialogResultTypes.Cancel;
            var result = new CopyStrategyWhenExistsResult(ExistsCopyFileActions.Skip, "", false);

            if (CancellationTokenSource.IsCancellationRequested)
                return result;

            using var m = new ManualResetEventSlim();

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var message = await _arg.Messenger.RaiseAsync(
                    new SelectFileCopyActionMessage(
                        srcPath,
                        destPath,
                        WindowBaseViewModel.MessageKeySelectFileCopyAction));

                resultDialogResult = resultDialogResult = message.Response.DialogResult;
                result = message.Response.Result;

                // ReSharper disable once AccessToDisposedClosure
                m.Set();
            });

            m.Wait();

            if (resultDialogResult == DialogResultTypes.Cancel)
                CancellationTokenSource.Cancel();

            return result;
        }
    }

    protected override CopyStrategyWhenSamePathResult CopyStrategyWhenSamePath(
        string destPath)
    {
        lock (_lockObj)
        {
            Debug.Assert(CancellationTokenSource is not null);

            if (CancellationTokenSource.IsCancellationRequested)
                return new CopyStrategyWhenSamePathResult(SamePathCopyFileActions.Skip, "");

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

            return new CopyStrategyWhenSamePathResult(
                resultDialogResult == DialogResultTypes.Ok
                    ? SamePathCopyFileActions.Override
                    : SamePathCopyFileActions.Skip,
                resultFilePath);
        }
    }
}
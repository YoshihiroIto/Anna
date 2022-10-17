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

    protected override (bool IsSkip, string NewDestPath) CopyStrategyWhenSamePath(string destPath)
    {
        lock (_lockObj)
        {
            Debug.Assert(CopyCancellationTokenSource is not null);

            if (CopyCancellationTokenSource.IsCancellationRequested)
                return (true, "");

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
                CopyCancellationTokenSource.Cancel();

            return (
                resultDialogResult != DialogResultTypes.Ok,
                resultFilePath);
        }
    }
}
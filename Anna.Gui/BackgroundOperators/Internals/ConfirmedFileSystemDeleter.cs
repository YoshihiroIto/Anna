using Anna.Constants;
using Anna.DomainModel.FileSystem;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Localization;
using Anna.Service;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.BackgroundOperators.Internals;

internal sealed class ConfirmedFileSystemDeleter
    : FileSystemDeleter
        , IHasArg<(InteractionMessenger Messenger, int Dummy)>
{
    private readonly (InteractionMessenger Messenger, int Dummy) _arg;
    private readonly object _lockObj = new();

    public ConfirmedFileSystemDeleter(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out _arg);
    }

    protected override ReadOnlyDeleteActions DeleteActionWhenReadonly(FileSystemInfo info)
    {
        return ReadOnlyDeleteActions.Skip;
    }

    protected override AccessFailureDeleteActions DeleteActionWhenAccessFailure(FileSystemInfo info)
    {
        lock (_lockObj)
        {
            Debug.Assert(CancellationTokenSource is not null);

            if (CancellationTokenSource.IsCancellationRequested)
                return AccessFailureDeleteActions.Cancel;

            var resultDialogResult = DialogResultTypes.Cancel;

            using var m = new ManualResetEventSlim();

            Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var message = await _arg.Messenger.RaiseAsync(
                        new ConfirmationMessage(
                            Resources.AppName,
                            string.Format(Resources.Message_AccessFailureOnDelete, info.FullName),
                        ConfirmationTypes.RetryCancel,
                        WindowBaseViewModel.MessageKeyConfirmation));

                resultDialogResult = message.Response;

                // ReSharper disable once AccessToDisposedClosure
                m.Set();
            });

            m.Wait();

            switch (resultDialogResult)
            {
                case DialogResultTypes.Retry:
                    return AccessFailureDeleteActions.Retry;
                
                case DialogResultTypes.Cancel:
                    CancellationTokenSource.Cancel();
                    return AccessFailureDeleteActions.Cancel;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
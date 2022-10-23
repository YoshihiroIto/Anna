using Anna.Constants;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Localization;
using Anna.Service;
using System;
using System.Diagnostics;
using System.IO;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.BackgroundOperators.Internals;

internal sealed class ConfirmedFileSystemDeleter
    : FileSystemDeleter
        , IHasArg<(InteractionMessenger Messenger, int Dummy)>
{
    private readonly (InteractionMessenger Messenger, int Dummy) _arg;
    private FastSpinLock _lockObj;

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
        Debug.Assert(CancellationTokenSource is not null);

        try
        {
            _lockObj.Enter();

            if (CancellationTokenSource.IsCancellationRequested)
                return AccessFailureDeleteActions.Cancel;

            var message = _arg.Messenger.Raise(
                new ConfirmationMessage(
                    Resources.AppName,
                    string.Format(Resources.Message_AccessFailureOnDelete, info.FullName),
                    DialogResultTypes.Retry | DialogResultTypes.Cancel,
                    WindowBaseViewModel.MessageKeyConfirmation));

            switch (message.Response)
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
        finally
        {
            _lockObj.Exit();
        }
    }
}
using Anna.Constants;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Gui.Views.Windows.Dialogs;
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

    protected override void DeleteActionWhenReadonly(FileSystemInfo info, ref ReadOnlyDeleteActions action)
    {
        Debug.Assert(CancellationTokenSource is not null);

        try
        {
            _lockObj.Enter();

            if (action == ReadOnlyDeleteActions.AllDelete)
                return;

            if (CancellationTokenSource.IsCancellationRequested)
            {
                action = ReadOnlyDeleteActions.Cancel;
                return;
            }

            using var viewModel =
                Dic.GetInstance<ConfirmationDialogViewModel, (string, string, DialogResultTypes)>((
                    Resources.AppName,
                    string.Format(Resources.Message_ReadOnlyConfirmDelete, info.FullName),
                    DialogResultTypes.Yes |
                    DialogResultTypes.No |
                    DialogResultTypes.AllDelete |
                    DialogResultTypes.Cancel
                ));

            _arg.Messenger.Raise(new TransitionMessage(viewModel, WindowBaseViewModel.MessageKeyConfirmation));

            switch (viewModel.DialogResult)
            {
                case DialogResultTypes.Yes:
                    action = ReadOnlyDeleteActions.Delete;
                    break;

                case DialogResultTypes.No:
                    action = ReadOnlyDeleteActions.Skip;
                    break;

                case DialogResultTypes.AllDelete:
                    action = ReadOnlyDeleteActions.AllDelete;
                    break;

                case DialogResultTypes.Cancel:
                    action = ReadOnlyDeleteActions.Cancel;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        finally
        {
            _lockObj.Exit();
        }
    }

    protected override AccessFailureDeleteActions DeleteActionWhenAccessFailure(FileSystemInfo info)
    {
        Debug.Assert(CancellationTokenSource is not null);

        try
        {
            _lockObj.Enter();

            if (CancellationTokenSource.IsCancellationRequested)
                return AccessFailureDeleteActions.Cancel;

            using var viewModel =
                Dic.GetInstance<ConfirmationDialogViewModel, (string, string, DialogResultTypes)>((
                    Resources.AppName,
                    string.Format(Resources.Message_AccessFailureOnDelete, info.FullName),
                    DialogResultTypes.Retry | DialogResultTypes.Cancel
                ));

            _arg.Messenger.Raise(new TransitionMessage(viewModel, WindowBaseViewModel.MessageKeyConfirmation));

            switch (viewModel.DialogResult)
            {
                case DialogResultTypes.Retry:
                    return AccessFailureDeleteActions.Retry;

                case DialogResultTypes.Cancel:
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
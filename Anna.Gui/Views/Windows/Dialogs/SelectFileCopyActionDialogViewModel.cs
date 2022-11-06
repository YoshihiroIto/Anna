using Anna.Constants;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Localization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Windows.Input;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class SelectFileCopyActionDialogViewModel : HasModelWindowBaseViewModel<
    SelectFileCopyActionDialogViewModel,
    (string SrcFilePath, string DestFilePath, bool IsSameActionThereafter)>
{
    public FileSystemCopier.CopyActionWhenExistsResult Result { get; private set; } =
        new(ExistsCopyFileActions.Skip, "", false, false);

    public string FileName => Path.GetFileName(Model.SrcFilePath);

    public string SrcFolder { get; }
    public DateTime SrcTimeStamp { get; }
    public string SrcSize { get; }

    public string DestFolder { get; }
    public DateTime DestTimeStamp { get; }
    public string DestSize { get; }

    public ReactivePropertySlim<bool> IsSameActionThereafter { get; }

    public ICommand CopyWhenTimestampIsNewestCommand { get; }
    public ICommand CopyOverrideCommand { get; }
    public ICommand SkipCommand { get; }
    public ICommand RenameAndCopyCommand { get; }
    public ICommand CancelCommand { get; }

    public SelectFileCopyActionDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        var newInfo = new FileInfo(Model.SrcFilePath);
        var destInfo = new FileInfo(Model.DestFilePath);

        SrcFolder = Path.GetDirectoryName(Model.SrcFilePath) ?? "";
        SrcTimeStamp = newInfo.LastWriteTime;
        SrcSize = StringHelper.MakeSizeString(newInfo.Length);

        DestFolder = Path.GetDirectoryName(Model.DestFilePath) ?? "";
        DestTimeStamp = destInfo.LastWriteTime;
        DestSize = StringHelper.MakeSizeString(destInfo.Length);

        IsSameActionThereafter = new ReactivePropertySlim<bool>(Model.IsSameActionThereafter).AddTo(Trash);

        CopyWhenTimestampIsNewestCommand =
            CreateButtonCommand(ExistsCopyFileActions.NewerTimestamp, DialogResultTypes.Ok);
        CopyOverrideCommand = CreateButtonCommand(ExistsCopyFileActions.Override, DialogResultTypes.Ok);
        SkipCommand = CreatSkipCommand();
        RenameAndCopyCommand = CreateRenameAndCopyCommand();
        CancelCommand = CreateButtonCommand(DialogResultTypes.Cancel);
    }

    private ICommand CreateButtonCommand(ExistsCopyFileActions action, DialogResultTypes result)
    {
        return new AsyncDelegateCommand(async () =>
        {
            DialogResult = result;
            Result = new FileSystemCopier.CopyActionWhenExistsResult(action, "", IsSameActionThereafter.Value, false);

            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));
        });
    }

    private ICommand CreatSkipCommand()
    {
        return IsSameActionThereafter
            .Inverse()
            .ToAsyncReactiveCommand()
            .WithSubscribe(async () =>
            {
                DialogResult = DialogResultTypes.Skip;
                await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));
            });
    }

    private ICommand CreateRenameAndCopyCommand()
    {
        return IsSameActionThereafter
            .Inverse()
            .ToAsyncReactiveCommand()
            .WithSubscribe(async () =>
            {
                using var viewModel = await Messenger.RaiseTransitionAsync(
                    InputEntryNameDialogViewModel.T,
                    (DestFolder, Path.GetFileName(Model.DestFilePath), Resources.DialogTitle_ChangeEntryName, true,
                        true),
                    MessageKey.InputEntryName);

                switch (viewModel.DialogResult)
                {
                    case DialogResultTypes.Ok:
                        DialogResult = DialogResultTypes.Ok;
                        Result = new FileSystemCopier.CopyActionWhenExistsResult(
                            ExistsCopyFileActions.Rename,
                            viewModel.ResultFilePath,
                            IsSameActionThereafter.Value,
                            false);

                        await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));

                        break;

                    case DialogResultTypes.Cancel:
                        // do nothing
                        break;

                    case DialogResultTypes.Skip:
                        DialogResult = DialogResultTypes.Skip;
                        Result = new FileSystemCopier.CopyActionWhenExistsResult(
                            ExistsCopyFileActions.Skip,
                            "",
                            IsSameActionThereafter.Value,
                            false);

                        await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).AddTo(Trash);
    }
}
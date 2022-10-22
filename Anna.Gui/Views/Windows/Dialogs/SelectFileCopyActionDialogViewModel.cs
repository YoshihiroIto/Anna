using Anna.Constants;
using Anna.DomainModel.FileSystem;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Windows.Input;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class SelectFileCopyActionDialogViewModel
    : HasModelWindowBaseViewModel<(string SrcFilepath, string DestFilepath)>
{
    public FileSystemCopier.CopyActionWhenExistsResult Result { get; private set; } =
        new(ExistsCopyFileActions.Skip, "", false);

    public string Filename => Path.GetFileName(Model.SrcFilepath);

    public string SrcFolder { get; }
    public DateTime SrcTimeStamp { get; }
    public string SrcSize { get; }

    public string DestFolder { get; }
    public DateTime DestTimeStamp { get; }
    public string DestSize { get; }

    public ReactivePropertySlim<bool> IsSameActionThereafter { get; }

    public ICommand CopyWhenTimestampIsNewestCommand { get; }
    public ICommand CopyOverrideCommand { get; }
    public ICommand RenameAndCopyCommand { get; }

    public SelectFileCopyActionDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        var newInfo = new FileInfo(Model.SrcFilepath);
        var destInfo = new FileInfo(Model.DestFilepath);

        SrcFolder = Path.GetDirectoryName(Model.SrcFilepath) ?? "";
        SrcTimeStamp = newInfo.LastWriteTime;
        SrcSize = StringHelper.MakeSizeString(newInfo.Length);

        DestFolder = Path.GetDirectoryName(Model.DestFilepath) ?? "";
        DestTimeStamp = destInfo.LastWriteTime;
        DestSize = StringHelper.MakeSizeString(destInfo.Length);

        IsSameActionThereafter = new ReactivePropertySlim<bool>(true).AddTo(Trash);

        CopyWhenTimestampIsNewestCommand =
            CreateButtonCommand(ExistsCopyFileActions.NewerTimestamp, DialogResultTypes.Ok);
        CopyOverrideCommand = CreateButtonCommand(ExistsCopyFileActions.Override, DialogResultTypes.Ok);

        _SkipCommand = CreatSkipCommand();
        RenameAndCopyCommand = CreateRenameAndCopyCommand();
    }

    private ICommand CreateButtonCommand(ExistsCopyFileActions action, DialogResultTypes result)
    {
        return new AsyncDelegateCommand(async () =>
        {
            DialogResult = result;
            Result = new FileSystemCopier.CopyActionWhenExistsResult(action, "", IsSameActionThereafter.Value);

            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
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
                await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
            });
    }

    private ICommand CreateRenameAndCopyCommand()
    {
        return IsSameActionThereafter
            .Inverse()
            .ToAsyncReactiveCommand()
            .WithSubscribe(async () =>
            {
                var changeNameMessage = await Messenger.RaiseAsync(
                    new ChangeEntryNameMessage(
                        DestFolder,
                        Path.GetFileName(Model.DestFilepath),
                        MessageKeyChangeEntryName));

                switch (changeNameMessage.Response.DialogResult)
                {
                    case DialogResultTypes.Ok:
                        DialogResult = DialogResultTypes.Ok;
                        Result = new FileSystemCopier.CopyActionWhenExistsResult(
                            ExistsCopyFileActions.Rename,
                            changeNameMessage.Response.FilePath,
                            IsSameActionThereafter.Value);

                        await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));

                        break;

                    case DialogResultTypes.Cancel:
                        // do nothing
                        break;

                    case DialogResultTypes.Skip:
                        DialogResult = DialogResultTypes.Skip;
                        Result = new FileSystemCopier.CopyActionWhenExistsResult(ExistsCopyFileActions.Skip,
                            "",
                            IsSameActionThereafter.Value);

                        await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).AddTo(Trash);
    }
}
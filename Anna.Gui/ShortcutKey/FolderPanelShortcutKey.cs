using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Gui.BackgroundOperators;
using Anna.Gui.BackgroundOperators.Internals;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Gui.Views.Windows.Dialogs;
using Anna.Localization;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.ShortcutKey;

public sealed class FolderPanelShortcutKey : ShortcutKeyBase
{
    public FolderPanelShortcutKey(IServiceProvider dic)
        : base(dic)
    {
    }

    protected override IReadOnlyDictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IShortcutKeyReceiver, ValueTask>>
        {
            { Operations.SortEntry, SelectSortModeAndOrderAsync },
            //
            { Operations.MoveCursorUp, s => MoveCursorAsync(s, Directions.Up) },
            { Operations.MoveCursorDown, s => MoveCursorAsync(s, Directions.Down) },
            { Operations.MoveCursorLeft, s => MoveCursorAsync(s, Directions.Left) },
            { Operations.MoveCursorRight, s => MoveCursorAsync(s, Directions.Right) },
            //
            { Operations.ToggleSelectionCursorEntry, s => ToggleSelectionCursorEntryAsync(s, true) },
            //
            { Operations.JumpFolder, JumpFolderAsync },
            { Operations.JumpToParentFolder, JumpToParentFolderAsync },
            { Operations.JumpToRootFolder, JumpToRootFolderAsync },
            //
            { Operations.OpenEntry, OpenEntryAsync },
            { Operations.OpenEntryByEditor1, s => OpenEntryByEditorAsync(s, 1) },
            { Operations.OpenEntryByEditor2, s => OpenEntryByEditorAsync(s, 2) },
            { Operations.OpenEntryByApp, OpenEntryByAppAsync },
            //
            { Operations.CopyEntry, s => CopyOrMoveEntryAsync(CopyOrMove.Copy, s) },
            { Operations.MoveEntry, s => CopyOrMoveEntryAsync(CopyOrMove.Move, s) },
            { Operations.DeleteEntry, DeleteEntryAsync },
            { Operations.RenameEntry, RenameEntryAsync },
            //
            { Operations.MakeFolder, s => MakeFolderOrFileAsync(true, s) },
            { Operations.MakeFile, s => MakeFolderOrFileAsync(false, s) },
            //
            { Operations.CompressEntry, CompressEntryAsync },
            { Operations.DecompressEntry, DecompressEntryAsync },
            //
            { Operations.EmptyTrashCan, EmptyTrashCanAsync },
            { Operations.OpenTrashCan, OpenTrashCanAsync },
        };
    }

    private async ValueTask SelectSortModeAndOrderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        using var viewModel = Dic.GetInstance<SortModeAndOrderDialogViewModel>();

        await receiver.Messenger.RaiseAsync(new TransitionMessage(viewModel, WindowBaseViewModel.MessageKeySelectSortModeAndOrder));

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        receiver.Folder.SetSortModeAndOrder(viewModel.ResultSortMode, viewModel.ResultSortOrder);
    }

    private async ValueTask JumpFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;
        var currentFolderPath = receiver.Owner.ViewModel.Model.Path;

        using var viewModel =
            Dic.GetInstance<JumpFolderDialogViewModel, (string, JumpFolderConfigData )>(
                (currentFolderPath, Dic.GetInstance<JumpFolderConfig>().Data));

        await receiver.Messenger.RaiseAsync(new TransitionMessage(viewModel, WindowBaseViewModel.MessageKeyJumpFolder));
        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        if (await CheckIsAccessibleAsync(viewModel.ResultPath, receiver.Messenger) == false)
            return;

        receiver.Folder.Path = viewModel.ResultPath;
    }

    private static ValueTask MoveCursorAsync(IShortcutKeyReceiver shortcutKeyReceiver, Directions dir)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        receiver.MoveCursor(dir);
        return ValueTask.CompletedTask;
    }

    private static ValueTask ToggleSelectionCursorEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver, bool isMoveDown)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        receiver.ToggleSelectionCursorEntry(isMoveDown);
        return ValueTask.CompletedTask;
    }

    private async ValueTask OpenEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;
        var target = receiver.CurrentEntry;

        if (await CheckIsAccessibleAsync(target.Path, receiver.Messenger) == false)
            return;

        if (target.IsFolder)
        {
            receiver.Folder.Path = target.Path;
        }
        else
        {
            using var viewModel = Dic.GetInstance<EntryDisplayDialogViewModel, Entry>(target);

            await receiver.Messenger.RaiseAsync(new TransitionMessage(viewModel,
                WindowBaseViewModel.MessageKeyEntryDisplay));
        }
    }

    private ValueTask OpenEntryByEditorAsync(IShortcutKeyReceiver shortcutKeyReceiver, int index)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        return receiver.CurrentEntry.IsFolder
            ? ValueTask.CompletedTask
            : OpenFileByEditorAsync(index, receiver.CurrentEntry.Path, 1, receiver.Messenger);
    }

    private ValueTask OpenEntryByAppAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        return StartAssociatedAppAsync(receiver.CurrentEntry.Path, receiver.Messenger);
    }

    private async ValueTask JumpToParentFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        var parentDir = new DirectoryInfo(receiver.Folder.Path).Parent?.FullName;
        if (parentDir is null)
            return;

        if (await CheckIsAccessibleAsync(receiver.Folder.Path, receiver.Messenger) == false)
            return;

        receiver.Folder.Path = parentDir;
    }
    private async ValueTask JumpToRootFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        var rootDir = Path.GetPathRoot(receiver.Folder.Path);
        if (rootDir is null)
            return;

        if (await CheckIsAccessibleAsync(rootDir, receiver.Messenger) == false)
            return;

        receiver.Folder.Path = rootDir;
    }

    private async ValueTask CopyOrMoveEntryAsync(CopyOrMove copyOrMove, IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;
        if (receiver.TargetEntries.Length == 0)
            return;

        var stats = Dic.GetInstance<EntriesStats, Entry[]>(receiver.TargetEntries);

        using var viewModel =
            Dic.GetInstance<CopyOrMoveEntryDialogViewModel,
                (CopyOrMove, string, Entry[], EntriesStats, ReadOnlyObservableCollection<string>)>
            ((
                copyOrMove,
                receiver.Folder.Path,
                receiver.TargetEntries,
                stats,
                Dic.GetInstance<IFolderHistoryService>().DestinationFolders
            ));

        await receiver.Messenger.RaiseAsync(new TransitionMessage(viewModel,
            WindowBaseViewModel.MessageKeyCopyOrMoveEntry));

        if (viewModel.DialogResult != DialogResultTypes.Ok)
        {
            stats.Dispose();
            return;
        }

        var worker =
            Dic.GetInstance<ConfirmedFileSystemCopier, (InteractionMessenger, CopyOrMove)>((receiver.Messenger,
                copyOrMove));

        var destFolder = Path.IsPathRooted(viewModel.ResultDestFolder)
            ? viewModel.ResultDestFolder
            : Path.Combine(receiver.Folder.Path, viewModel.ResultDestFolder);

        destFolder = PathStringHelper.Normalize(destFolder);

        var targetEntries = receiver.TargetEntries;

        var @operator = Dic.GetInstance<EntryBackgroundOperator, (IEntriesStats, IFileProcessable, Action)>
        ((
            stats,
            worker,
            () => worker.Invoke(targetEntries, destFolder)
        ));

        await receiver.BackgroundWorker.PushOperatorAsync(@operator);

        Dic.GetInstance<IFolderHistoryService>().AddDestinationFolder(destFolder);
    }

    private async ValueTask DeleteEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;
        if (receiver.TargetEntries.Length == 0)
            return;

        var stats = Dic.GetInstance<EntriesStats, Entry[]>(receiver.TargetEntries);

        using var viewModel =
            Dic.GetInstance<DeleteEntryDialogViewModel, (Entry[], EntriesStats)>
                ((receiver.TargetEntries, stats));

        await receiver.Messenger.RaiseAsync(new TransitionMessage(viewModel,
            WindowBaseViewModel.MessageKeyDeleteEntry));

        if (viewModel.DialogResult != DialogResultTypes.Yes)
        {
            stats.Dispose();
            return;
        }

        var worker = Dic.GetInstance<ConfirmedFileSystemDeleter, (InteractionMessenger, int)>((receiver.Messenger, 0));
        var targetEntries = receiver.TargetEntries;

        var @operator = Dic.GetInstance<EntryBackgroundOperator, (IEntriesStats, IFileProcessable, Action)>
        ((
            stats,
            worker,
            // ReSharper disable once AccessToDisposedClosure
            () => worker.Invoke(targetEntries, viewModel.ResultMode)
        ));

        await receiver.BackgroundWorker.PushOperatorAsync(@operator);
    }

    private async ValueTask RenameEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;
        var targetEntries = receiver.TargetEntries;

        string? lastRemovePath = null;

        foreach (var targetEntry in targetEntries)
        {
            using var viewModel =
                Dic.GetInstance<InputEntryNameDialogViewModel, (string, string, string, bool, bool)>(
                    (receiver.Folder.Path, targetEntry.NameWithExtension, Resources.DialogTitle_Rename, false, true));

            await receiver.Messenger.RaiseAsync(new TransitionMessage(viewModel,
                WindowBaseViewModel.MessageKeyInputEntryName));

            switch (viewModel.DialogResult)
            {
                case DialogResultTypes.Cancel:
                    return;

                case DialogResultTypes.Skip:
                    // do nothing
                    break;

                case DialogResultTypes.Ok:
                    {
                        var fileName = Path.GetFileName(viewModel.ResultFilePath);
                        receiver.Folder.RenameEntry(targetEntry, fileName, false);

                        lastRemovePath = fileName;
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (lastRemovePath is not null)
            receiver.Folder.InvokeEntryExplicitlyCreated(lastRemovePath);
    }

    private async ValueTask MakeFolderOrFileAsync(bool isFolder, IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        using var viewModel =
            Dic.GetInstance<InputEntryNameDialogViewModel, (string, string, string, bool, bool)>(
                (receiver.Folder.Path,
                    FileSystemHelper.MakeNewEntryName(
                        receiver.Folder.Path,
                        isFolder ? Resources.Entry_NewFolder : Resources.Entry_NewFile),
                    isFolder ? Resources.DialogTitle_CreateFolder : Resources.DialogTitle_CreateFile,
                    false,
                    false));

        await receiver.Messenger.RaiseAsync(new TransitionMessage(viewModel,
            WindowBaseViewModel.MessageKeyInputEntryName));

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        receiver.Folder.CreateEntry(isFolder, viewModel.ResultFilePath, true);
    }

    private static ValueTask CompressEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        throw new NotImplementedException();
    }

    private static ValueTask DecompressEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        throw new NotImplementedException();
    }

    private async ValueTask EmptyTrashCanAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        var info = Dic.GetInstance<ITrashCanService>().GetTrashCanInfo();
        if (info.EntryCount == 0)
            return;

        var confirmText = info.EntryCount == 1
            ? Resources.Messege_ConfirmEmptyTrashCan_Single
            : Resources.Messege_ConfirmEmptyTrashCan_Multi;

        using var viewModel =
            Dic.GetInstance<ConfirmationDialogViewModel, (string, string, DialogResultTypes)>((
                Resources.AppName,
                string.Format(confirmText, info.EntryCount.ToString()),
                DialogResultTypes.OpenTrashCan | DialogResultTypes.Yes | DialogResultTypes.No
            ));

        await shortcutKeyReceiver.Messenger.RaiseAsync(new TransitionMessage(viewModel,
            WindowBaseViewModel.MessageKeyConfirmation));

        switch (viewModel.DialogResult)
        {
            case DialogResultTypes.OpenTrashCan:
                Dic.GetInstance<ITrashCanService>().OpenTrashCan();
                break;

            case DialogResultTypes.Yes:
                var @operator = Dic.GetInstance<DelegateBackgroundOperator, Action>(
                    () => Dic.GetInstance<ITrashCanService>().EmptyTrashCan());

                await receiver.BackgroundWorker.PushOperatorAsync(@operator);
                break;

            case DialogResultTypes.No:
            case DialogResultTypes.Cancel:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private ValueTask OpenTrashCanAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        Dic.GetInstance<ITrashCanService>().OpenTrashCan();
        return ValueTask.CompletedTask;
    }
}
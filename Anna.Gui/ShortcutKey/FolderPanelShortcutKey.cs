using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Gui.BackgroundOperators;
using Anna.Gui.BackgroundOperators.Internals;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Windows;
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
            { Operations.SortEntry, s => SelectSortModeAndOrderAsync((IFolderPanelShortcutKeyReceiver)s) },
            //
            { Operations.MoveCursorUp, s => MoveCursorAsync((IFolderPanelShortcutKeyReceiver)s, Directions.Up) },
            { Operations.MoveCursorDown, s => MoveCursorAsync((IFolderPanelShortcutKeyReceiver)s, Directions.Down) },
            { Operations.MoveCursorLeft, s => MoveCursorAsync((IFolderPanelShortcutKeyReceiver)s, Directions.Left) },
            { Operations.MoveCursorRight, s => MoveCursorAsync((IFolderPanelShortcutKeyReceiver)s, Directions.Right) },
            //
            {
                Operations.ToggleSelectionCursorEntry,
                s => ToggleSelectionCursorEntryAsync((IFolderPanelShortcutKeyReceiver)s, true)
            },
            //
            { Operations.JumpFolder, s => JumpFolderAsync((IFolderPanelShortcutKeyReceiver)s) },
            { Operations.JumpToParentFolder, s => JumpToParentFolderAsync((IFolderPanelShortcutKeyReceiver)s) },
            { Operations.JumpToRootFolder, s => JumpToRootFolderAsync((IFolderPanelShortcutKeyReceiver)s) },
            //
            { Operations.OpenEntry, s => OpenEntryAsync((IFolderPanelShortcutKeyReceiver)s) },
            { Operations.OpenEntryByEditor1, s => OpenEntryByEditorAsync((IFolderPanelShortcutKeyReceiver)s, 1) },
            { Operations.OpenEntryByEditor2, s => OpenEntryByEditorAsync((IFolderPanelShortcutKeyReceiver)s, 2) },
            { Operations.OpenEntryByApp, s => OpenEntryByAppAsync((IFolderPanelShortcutKeyReceiver)s) },
            //
            { Operations.CopyEntry, s => CopyOrMoveEntryAsync((IFolderPanelShortcutKeyReceiver)s, CopyOrMove.Copy) },
            { Operations.MoveEntry, s => CopyOrMoveEntryAsync((IFolderPanelShortcutKeyReceiver)s, CopyOrMove.Move) },
            { Operations.DeleteEntry, s => DeleteEntryAsync((IFolderPanelShortcutKeyReceiver)s) },
            { Operations.RenameEntry, s => RenameEntryAsync((IFolderPanelShortcutKeyReceiver)s) },
            //
            { Operations.MakeFolder, s => MakeFolderOrFileAsync((IFolderPanelShortcutKeyReceiver)s, true) },
            { Operations.MakeFile, s => MakeFolderOrFileAsync((IFolderPanelShortcutKeyReceiver)s, false) },
            //
            { Operations.CompressEntry, s => CompressEntryAsync((IFolderPanelShortcutKeyReceiver)s) },
            { Operations.DecompressEntry, s => DecompressEntryAsync((IFolderPanelShortcutKeyReceiver)s) },
            //
            { Operations.EmptyTrashCan, s => EmptyTrashCanAsync((IFolderPanelShortcutKeyReceiver)s) },
            { Operations.OpenTrashCan, s => OpenTrashCanAsync((IFolderPanelShortcutKeyReceiver)s) },
        };
    }

    private async ValueTask SelectSortModeAndOrderAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        using var viewModel = await RaiseTransitionAsync(receiver.Messenger,
            Dic.GetInstance<SortModeAndOrderDialogViewModel>(),
            MessageKey.SelectSortModeAndOrder);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        receiver.Folder.SetSortModeAndOrder(viewModel.ResultSortMode, viewModel.ResultSortOrder);
    }

    private async ValueTask JumpFolderAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        var currentFolderPath = receiver.Owner.ViewModel.Model.Path;

        using var viewModel = await RaiseTransitionAsync(receiver.Messenger,
            Dic.GetInstance<JumpFolderDialogViewModel, (string, JumpFolderConfigData )>
                ((currentFolderPath, Dic.GetInstance<JumpFolderConfig>().Data)),
            MessageKey.JumpFolder);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        if (await CheckIsAccessibleAsync(viewModel.ResultPath, receiver.Messenger) == false)
            return;

        receiver.Folder.Path = viewModel.ResultPath;
    }

    private static ValueTask MoveCursorAsync(IFolderPanelShortcutKeyReceiver receiver, Directions dir)
    {
        receiver.MoveCursor(dir);

        return ValueTask.CompletedTask;
    }

    private static ValueTask ToggleSelectionCursorEntryAsync(IFolderPanelShortcutKeyReceiver receiver, bool isMoveDown)
    {
        receiver.ToggleSelectionCursorEntry(isMoveDown);

        return ValueTask.CompletedTask;
    }

    private async ValueTask OpenEntryAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        var target = receiver.CurrentEntry;

        if (await CheckIsAccessibleAsync(target.Path, receiver.Messenger) == false)
            return;

        if (target.IsFolder)
        {
            receiver.Folder.Path = target.Path;
        }
        else
        {
            using var viewModel = await RaiseTransitionAsync(receiver.Messenger,
                Dic.GetInstance<EntryDisplayDialogViewModel, Entry>(target),
                MessageKey.EntryDisplay);
        }
    }

    private ValueTask OpenEntryByEditorAsync(IFolderPanelShortcutKeyReceiver receiver, int index)
    {
        return receiver.CurrentEntry.IsFolder
            ? ValueTask.CompletedTask
            : OpenFileByEditorAsync(index, receiver.CurrentEntry.Path, 1, receiver.Messenger);
    }

    private ValueTask OpenEntryByAppAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        return StartAssociatedAppAsync(receiver.CurrentEntry.Path, receiver.Messenger);
    }

    private async ValueTask JumpToParentFolderAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        var parentDir = new DirectoryInfo(receiver.Folder.Path).Parent?.FullName;
        if (parentDir is null)
            return;

        if (await CheckIsAccessibleAsync(receiver.Folder.Path, receiver.Messenger) == false)
            return;

        receiver.Folder.Path = parentDir;
    }
    private async ValueTask JumpToRootFolderAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        var rootDir = Path.GetPathRoot(receiver.Folder.Path);
        if (rootDir is null)
            return;

        if (await CheckIsAccessibleAsync(rootDir, receiver.Messenger) == false)
            return;

        receiver.Folder.Path = rootDir;
    }

    private async ValueTask CopyOrMoveEntryAsync(IFolderPanelShortcutKeyReceiver receiver, CopyOrMove copyOrMove)
    {
        if (receiver.TargetEntries.Length == 0)
            return;

        var stats = Dic.GetInstance<EntriesStats, Entry[]>(receiver.TargetEntries);

        using var viewModel = await RaiseTransitionAsync(receiver.Messenger,
            Dic.GetInstance<CopyOrMoveEntryDialogViewModel,
                (CopyOrMove, string, Entry[], EntriesStats, ReadOnlyObservableCollection<string>)>
            ((
                copyOrMove,
                receiver.Folder.Path,
                receiver.TargetEntries,
                stats,
                Dic.GetInstance<IFolderHistoryService>().DestinationFolders
            )),
            MessageKey.CopyOrMoveEntry);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
        {
            stats.Dispose();
            return;
        }

        var worker =
            Dic.GetInstance<ConfirmedFileSystemCopier, (Messenger, CopyOrMove)>((receiver.Messenger,
                copyOrMove));

        var destFolder = Path.IsPathRooted(viewModel.ResultDestFolder)
            ? viewModel.ResultDestFolder
            : Path.Combine(receiver.Folder.Path, viewModel.ResultDestFolder);

        destFolder = PathStringHelper.Normalize(destFolder);

        var @operator = Dic.GetInstance<EntryBackgroundOperator, (IEntriesStats, IFileProcessable, Action)>
        ((
            stats,
            worker,
            () => worker.Invoke(receiver.TargetEntries, destFolder)
        ));

        await receiver.BackgroundWorker.PushOperatorAsync(@operator);

        Dic.GetInstance<IFolderHistoryService>().AddDestinationFolder(destFolder);
    }

    private async ValueTask DeleteEntryAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        if (receiver.TargetEntries.Length == 0)
            return;

        var stats = Dic.GetInstance<EntriesStats, Entry[]>(receiver.TargetEntries);

        using var viewModel = await RaiseTransitionAsync(receiver.Messenger,
            Dic.GetInstance<DeleteEntryDialogViewModel, (Entry[], EntriesStats)>
                ((receiver.TargetEntries, stats)),
            MessageKey.DeleteEntry);

        if (viewModel.DialogResult != DialogResultTypes.Yes)
        {
            stats.Dispose();
            return;
        }

        var worker = Dic.GetInstance<ConfirmedFileSystemDeleter, (Messenger, int)>((receiver.Messenger, 0));

        var @operator = Dic.GetInstance<EntryBackgroundOperator, (IEntriesStats, IFileProcessable, Action)>
        ((
            stats,
            worker,
            // ReSharper disable once AccessToDisposedClosure
            () => worker.Invoke(receiver.TargetEntries, viewModel.ResultMode)
        ));

        await receiver.BackgroundWorker.PushOperatorAsync(@operator);
    }

    private async ValueTask RenameEntryAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        string? lastRemovePath = null;

        foreach (var targetEntry in receiver.TargetEntries)
        {
            using var viewModel = await RaiseTransitionAsync(receiver.Messenger,
                Dic.GetInstance<InputEntryNameDialogViewModel, (string, string, string, bool, bool)>(
                    (receiver.Folder.Path, targetEntry.NameWithExtension, Resources.DialogTitle_Rename, false, true)),
                MessageKey.InputEntryName);

            if (viewModel.DialogResult != DialogResultTypes.Ok)
                return;

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

    private async ValueTask MakeFolderOrFileAsync(IFolderPanelShortcutKeyReceiver receiver, bool isFolder)
    {
        using var viewModel = await RaiseTransitionAsync(receiver.Messenger,
            Dic.GetInstance<InputEntryNameDialogViewModel, (string, string, string, bool, bool)>(
                (receiver.Folder.Path,
                    FileSystemHelper.MakeNewEntryName(
                        receiver.Folder.Path,
                        isFolder ? Resources.Entry_NewFolder : Resources.Entry_NewFile),
                    isFolder ? Resources.DialogTitle_CreateFolder : Resources.DialogTitle_CreateFile,
                    false,
                    false)),
            MessageKey.InputEntryName);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        receiver.Folder.CreateEntry(isFolder, viewModel.ResultFilePath, true);
    }

    private static ValueTask CompressEntryAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        throw new NotImplementedException();
    }

    private async ValueTask DecompressEntryAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        if (receiver.TargetEntries.Length == 0)
            return;

        using var stats = Dic.GetInstance<EntriesStats, Entry[]>(receiver.TargetEntries);

        using var viewModel = await RaiseTransitionAsync(receiver.Messenger,
            Dic.GetInstance<DecompressEntryDialogViewModel,
                (string, Entry[], EntriesStats, ReadOnlyObservableCollection<string>)>
            ((
                receiver.Folder.Path,
                receiver.TargetEntries,
                stats,
                Dic.GetInstance<IFolderHistoryService>().DestinationFolders
            )),
            MessageKey.DecompressEntry);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        var destFolder = Path.IsPathRooted(viewModel.ResultDestFolder)
            ? viewModel.ResultDestFolder
            : Path.Combine(receiver.Folder.Path, viewModel.ResultDestFolder);

        destFolder = PathStringHelper.Normalize(destFolder);

        DelegateBackgroundOperator? op = null;
        var @operator = Dic.GetInstance<DelegateBackgroundOperator, Action>(
            () =>
            {
                var targetEntries = receiver.TargetEntries;

                for (var i = 0; i != targetEntries.Length; ++i)
                {
                    Dic.GetInstance<ICompressionService>().Decompress(
                        targetEntries[i].Path,
                        destFolder,
                        p =>
                        {
                            // ReSharper disable AccessToModifiedClosure
                            if (op is not null)
                                op.Progress = (i + p) / targetEntries.Length;
                            // ReSharper restore AccessToModifiedClosure
                        }
                    );
                }
            });

        op = @operator;

        await receiver.BackgroundWorker.PushOperatorAsync(@operator);

        Dic.GetInstance<IFolderHistoryService>().AddDestinationFolder(destFolder);
    }

    private async ValueTask EmptyTrashCanAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        var info = Dic.GetInstance<ITrashCanService>().GetTrashCanInfo();
        if (info.EntryCount == 0)
            return;

        var confirmText = info.EntryCount == 1
            ? Resources.Messege_ConfirmEmptyTrashCan_Single
            : Resources.Messege_ConfirmEmptyTrashCan_Multi;

        using var viewModel = await RaiseTransitionAsync(receiver.Messenger,
            Dic.GetInstance<ConfirmationDialogViewModel, (string, string, DialogResultTypes)>((
                Resources.AppName,
                string.Format(confirmText, info.EntryCount.ToString()),
                DialogResultTypes.OpenTrashCan | DialogResultTypes.Yes | DialogResultTypes.No
            )),
            MessageKey.Confirmation);

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

    private ValueTask OpenTrashCanAsync(IFolderPanelShortcutKeyReceiver receiver)
    {
        Dic.GetInstance<ITrashCanService>().OpenTrashCan();
        return ValueTask.CompletedTask;
    }
}
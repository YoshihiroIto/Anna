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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Hotkey;

public sealed class FolderPanelHotkey : HotkeyBase
{
    public FolderPanelHotkey(IServiceProvider dic)
        : base(dic)
    {
    }

    protected override IReadOnlyDictionary<Operations, Func<IHotkeyReceiver, ValueTask>> SetupOperators()
    {
        return new Dictionary<Operations, Func<IHotkeyReceiver, ValueTask>>
        {
            { Operations.SortEntry, s => SelectSortModeAndOrderAsync((IFolderPanelHotkeyReceiver)s) },
            //
            { Operations.MoveCursorUp, s => MoveCursorAsync((IFolderPanelHotkeyReceiver)s, Directions.Up) },
            { Operations.MoveCursorDown, s => MoveCursorAsync((IFolderPanelHotkeyReceiver)s, Directions.Down) },
            { Operations.MoveCursorLeft, s => MoveCursorAsync((IFolderPanelHotkeyReceiver)s, Directions.Left) },
            { Operations.MoveCursorRight, s => MoveCursorAsync((IFolderPanelHotkeyReceiver)s, Directions.Right) },
            //
            {
                Operations.ToggleSelectionCursorEntry,
                s => ToggleSelectionCursorEntryAsync((IFolderPanelHotkeyReceiver)s, true)
            },
            //
            { Operations.JumpFolder, s => JumpFolderAsync((IFolderPanelHotkeyReceiver)s) },
            { Operations.JumpToParentFolder, s => JumpToParentFolderAsync((IFolderPanelHotkeyReceiver)s) },
            { Operations.JumpToRootFolder, s => JumpToRootFolderAsync((IFolderPanelHotkeyReceiver)s) },
            //
            { Operations.OpenEntry, s => OpenEntryAsync((IFolderPanelHotkeyReceiver)s) },
            { Operations.OpenApp1, s => OpenAppAsync((IFolderPanelHotkeyReceiver)s, AppConfigData.ExternalApp.App1) },
            { Operations.OpenApp2, s => OpenAppAsync((IFolderPanelHotkeyReceiver)s, AppConfigData.ExternalApp.App2) },
            { Operations.OpenAssociatedApp, s => OpenAssociatedAppAsync((IFolderPanelHotkeyReceiver)s) },
            {
                Operations.OpenTerminal,
                s => OpenAppAsync((IFolderPanelHotkeyReceiver)s, AppConfigData.ExternalApp.Terminal)
            },
            { Operations.PreviewEntry, s => PreviewEntryAsync((IFolderPanelHotkeyReceiver)s) },
            //
            { Operations.CopyEntry, s => CopyOrMoveEntryAsync((IFolderPanelHotkeyReceiver)s, CopyOrMove.Copy) },
            { Operations.MoveEntry, s => CopyOrMoveEntryAsync((IFolderPanelHotkeyReceiver)s, CopyOrMove.Move) },
            { Operations.DeleteEntry, s => DeleteEntryAsync((IFolderPanelHotkeyReceiver)s) },
            { Operations.RenameEntry, s => RenameEntryAsync((IFolderPanelHotkeyReceiver)s) },
            //
            { Operations.MakeFolder, s => MakeFolderOrFileAsync((IFolderPanelHotkeyReceiver)s, true) },
            { Operations.MakeFile, s => MakeFolderOrFileAsync((IFolderPanelHotkeyReceiver)s, false) },
            //
            { Operations.CompressEntry, s => CompressEntryAsync((IFolderPanelHotkeyReceiver)s) },
            { Operations.DecompressEntry, s => DecompressEntryAsync((IFolderPanelHotkeyReceiver)s) },
            //
            { Operations.EmptyTrashCan, s => EmptyTrashCanAsync((IFolderPanelHotkeyReceiver)s) },
            { Operations.OpenTrashCan, s => OpenTrashCanAsync((IFolderPanelHotkeyReceiver)s) },
            //
            { Operations.OpenAnna, s => OpenAnnaAsync((IFolderPanelHotkeyReceiver)s) },
            { Operations.CloseAnna, s => CloseAnnaAsync((IFolderPanelHotkeyReceiver)s) },
        };
    }

    private static async ValueTask SelectSortModeAndOrderAsync(IFolderPanelHotkeyReceiver receiver)
    {
        using var viewModel = await receiver.Messenger.RaiseTransitionAsync(
            SortModeAndOrderDialogViewModel.T,
            0,
            MessageKey.SelectSortModeAndOrder);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        receiver.Folder.SetSortModeAndOrder(viewModel.ResultSortMode, viewModel.ResultSortOrder);
    }

    private async ValueTask JumpFolderAsync(IFolderPanelHotkeyReceiver receiver)
    {
        var currentFolderPath = receiver.Folder.Path;

        using var viewModel = await receiver.Messenger.RaiseTransitionAsync(
            JumpFolderDialogViewModel.T,
            (currentFolderPath, Dic.GetInstance<JumpFolderConfig>().Data),
            MessageKey.JumpFolder);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        if (await CheckIsAccessibleAsync(viewModel.ResultPath, receiver.Messenger) == false)
            return;

        receiver.Folder.Path = viewModel.ResultPath;
    }

    private static ValueTask MoveCursorAsync(IFolderPanelHotkeyReceiver receiver, Directions dir)
    {
        receiver.MoveCursor(dir);

        return ValueTask.CompletedTask;
    }

    private static ValueTask ToggleSelectionCursorEntryAsync(IFolderPanelHotkeyReceiver receiver, bool isMoveDown)
    {
        receiver.ToggleSelectionCursorEntry(isMoveDown);

        return ValueTask.CompletedTask;
    }

    private async ValueTask OpenEntryAsync(IFolderPanelHotkeyReceiver receiver)
    {
        var target = receiver.CurrentEntry;

        if (await CheckIsAccessibleAsync(target.Path, receiver.Messenger) == false)
            return;

        if (target.IsFolder)
            receiver.Folder.Path = target.Path;
        else
            await PreviewEntryAsync(receiver);
    }

    private async ValueTask PreviewEntryAsync(IFolderPanelHotkeyReceiver receiver)
    {
        var target = receiver.CurrentEntry;

        if (target.IsFolder)
            return;

        if (await CheckIsAccessibleAsync(target.Path, receiver.Messenger) == false)
            return;

        using var _ = await receiver.Messenger.RaiseTransitionAsync(
            EntryPreviewDialogViewModel.T,
            (target, 0),
            MessageKey.PreviewDisplay);
    }

    private ValueTask OpenAppAsync(IFolderPanelHotkeyReceiver receiver, AppConfigData.ExternalApp app)
    {
        return OpenAppAsync(
            app,
            receiver.CurrentEntry.Path,
            receiver.Folder.Path,
            1,
            receiver.Messenger);
    }

    private ValueTask OpenAssociatedAppAsync(IFolderPanelHotkeyReceiver receiver)
    {
        return StartAssociatedAppAsync(receiver.CurrentEntry.Path, receiver.Messenger);
    }

    private ValueTask JumpToParentFolderAsync(IFolderPanelHotkeyReceiver receiver)
    {
        var parentFolder = receiver.Folder.FindParentPath();

        return MoveFolderAsync(parentFolder, receiver);
    }
    private ValueTask JumpToRootFolderAsync(IFolderPanelHotkeyReceiver receiver)
    {
        var rootFolder = receiver.Folder.FindRootPath();

        return MoveFolderAsync(rootFolder, receiver);
    }

    private async ValueTask MoveFolderAsync(string? destPath, IFolderPanelHotkeyReceiver receiver)
    {
        if (destPath is null)
            return;

        if (await CheckIsAccessibleAsync(destPath, receiver.Messenger) == false)
            return;

        receiver.Folder.Path = destPath;
    }

    private async ValueTask CopyOrMoveEntryAsync(IFolderPanelHotkeyReceiver receiver, CopyOrMove copyOrMove)
    {
        if (receiver.TargetEntries.Length == 0)
            return;

        var stats = Dic.GetInstance(EntriesStats.T, receiver.TargetEntries);

        using var viewModel = await receiver.Messenger.RaiseTransitionAsync(
            CopyOrMoveEntryDialogViewModel.T,
            (copyOrMove, receiver.Folder.Path, receiver.TargetEntries, stats),
            MessageKey.CopyOrMoveEntry);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
        {
            stats.Dispose();
            return;
        }

        var destFolder = PathStringHelper.MakeFullPath(viewModel.ResultDestFolder, receiver.Folder.Path);
        var worker = Dic.GetInstance(ConfirmedFileSystemCopier.T,
            (receiver.Messenger, receiver.TargetEntries.Cast<IEntry>(), destFolder, copyOrMove));

        var @operator = Dic.GetInstance(EntryBackgroundOperator.T,
            ((IEntriesStats)stats, (IFileProcessable)worker, EntryBackgroundOperator.ProgressModes.Stats));
        receiver.BackgroundWorker.PushOperatorAsync(@operator).Forget();
    }

    private async ValueTask DeleteEntryAsync(IFolderPanelHotkeyReceiver receiver)
    {
        if (receiver.TargetEntries.Length == 0)
            return;

        var stats = Dic.GetInstance(EntriesStats.T, receiver.TargetEntries);

        using var viewModel = await receiver.Messenger.RaiseTransitionAsync(
            DeleteEntryDialogViewModel.T,
            (receiver.TargetEntries, stats),
            MessageKey.DeleteEntry);

        if (viewModel.DialogResult != DialogResultTypes.Yes)
        {
            stats.Dispose();
            return;
        }

        var resultMode = viewModel.ResultMode;
        var worker = Dic.GetInstance(ConfirmedFileSystemDeleter.T,
            (receiver.Messenger, receiver.TargetEntries.Cast<IEntry>(), resultMode));

        var @operator = Dic.GetInstance(EntryBackgroundOperator.T,
            ((IEntriesStats)stats, (IFileProcessable)worker, EntryBackgroundOperator.ProgressModes.Stats));
        receiver.BackgroundWorker.PushOperatorAsync(@operator).Forget();
    }

    private static async ValueTask RenameEntryAsync(IFolderPanelHotkeyReceiver receiver)
    {
        string? lastRemovePath = null;

        foreach (var targetEntry in receiver.TargetEntries)
        {
            using var viewModel = await receiver.Messenger.RaiseTransitionAsync(
                InputEntryNameDialogViewModel.T,
                (receiver.Folder.Path, targetEntry.NameWithExtension, Resources.DialogTitle_Rename, false, true),
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

    private static async ValueTask MakeFolderOrFileAsync(IFolderPanelHotkeyReceiver receiver, bool isFolder)
    {
        var newName = FileSystemHelper.MakeNewEntryName(
            receiver.Folder.Path,
            isFolder ? Resources.Entry_NewFolder : Resources.Entry_NewFile);
        var title = isFolder ? Resources.DialogTitle_CreateFolder : Resources.DialogTitle_CreateFile;

        using var viewModel = await receiver.Messenger.RaiseTransitionAsync(
            InputEntryNameDialogViewModel.T,
            (receiver.Folder.Path, newName, title, false, false),
            MessageKey.InputEntryName);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        receiver.Folder.CreateEntry(isFolder, viewModel.ResultFilePath, true);
    }

    private async ValueTask CompressEntryAsync(IFolderPanelHotkeyReceiver receiver)
    {
        if (receiver.TargetEntries.Length == 0)
            return;

        var stats = Dic.GetInstance(EntriesStats.T, receiver.TargetEntries);

        using var viewModel = await receiver.Messenger.RaiseTransitionAsync(
            CompressEntryDialogViewModel.T,
            (receiver.Folder.Path, receiver.TargetEntries, stats),
            MessageKey.CompressEntry);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
        {
            stats.Dispose();
            return;
        }

        var destFolder = PathStringHelper.MakeFullPath(viewModel.ResultDestFolder, receiver.Folder.Path);
        var destArchivePath = Path.Combine(destFolder, viewModel.ResultDestArchiveName);

        var worker = Dic.GetInstance(FileSystemCompressor.T,
            (receiver.TargetEntries.Select(x => x.Path), destArchivePath));

        var @operator = Dic.GetInstance(EntryBackgroundOperator.T,
            ((IEntriesStats)stats, (IFileProcessable)worker, EntryBackgroundOperator.ProgressModes.Stats));
        receiver.BackgroundWorker.PushOperatorAsync(@operator).Forget();
    }

    private async ValueTask DecompressEntryAsync(IFolderPanelHotkeyReceiver receiver)
    {
        if (receiver.TargetEntries.Length == 0)
            return;

        using var stats = Dic.GetInstance(EntriesStats.T, receiver.TargetEntries);

        using var viewModel = await receiver.Messenger.RaiseTransitionAsync(
            DecompressEntryDialogViewModel.T,
            (receiver.Folder.Path, receiver.TargetEntries, stats),
            MessageKey.DecompressEntry);

        if (viewModel.DialogResult != DialogResultTypes.Ok)
            return;

        var destFolder = PathStringHelper.MakeFullPath(viewModel.ResultDestFolder, receiver.Folder.Path);

        var worker = Dic.GetInstance(FileSystemDecompressor.T,
            (receiver.TargetEntries.Select(x => x.Path), destFolder));

        var @operator = Dic.GetInstance(EntryBackgroundOperator.T,
            (NopEntriesStats.Instance, (IFileProcessable)worker, EntryBackgroundOperator.ProgressModes.Direct));
        receiver.BackgroundWorker.PushOperatorAsync(@operator).Forget();
    }

    private async ValueTask EmptyTrashCanAsync(IFolderPanelHotkeyReceiver receiver)
    {
        var info = Dic.GetInstance<ITrashCanService>().GetTrashCanInfo();
        if (info.EntryCount == 0)
            return;

        var confirmText = string.Format(
            info.EntryCount == 1
                ? Resources.Messege_ConfirmEmptyTrashCan_Single
                : Resources.Messege_ConfirmEmptyTrashCan_Multi,
            info.EntryCount.ToString());

        using var viewModel = await receiver.Messenger.RaiseTransitionAsync(ConfirmationDialogViewModel.T,
            (Resources.AppName, confirmText,
                DialogResultTypes.OpenTrashCan | DialogResultTypes.Yes | DialogResultTypes.No),
            MessageKey.Confirmation);

        switch (viewModel.DialogResult)
        {
            case DialogResultTypes.OpenTrashCan:
                Dic.GetInstance<ITrashCanService>().OpenTrashCan();
                break;

            case DialogResultTypes.Yes:
                var @operator = Dic.GetInstance(DelegateBackgroundOperator.T,
                    (Action)(() => Dic.GetInstance<ITrashCanService>().EmptyTrashCan()));

                receiver.BackgroundWorker.PushOperatorAsync(@operator).Forget();
                break;

            case DialogResultTypes.No:
            case DialogResultTypes.Cancel:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private ValueTask OpenTrashCanAsync(IFolderPanelHotkeyReceiver receiver)
    {
        Dic.GetInstance<ITrashCanService>().OpenTrashCan();
        return ValueTask.CompletedTask;
    }

    private ValueTask OpenAnnaAsync(IFolderPanelHotkeyReceiver receiver)
    {
        Dic.GetInstance<App>().AddFolder(receiver.Folder.Path);
        return ValueTask.CompletedTask;
    }

    private ValueTask CloseAnnaAsync(IFolderPanelHotkeyReceiver receiver)
    {
        Dic.GetInstance<App>().RemoveFolder(receiver.Folder);
        return ValueTask.CompletedTask;
    }
}
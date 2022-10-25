using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Gui.BackgroundOperators;
using Anna.Gui.BackgroundOperators.Internals;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Windows;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using System;
using System.Collections.Generic;
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
            { Operations.JumpFolder, JumpFolderAsync },
            { Operations.MoveCursorUp, s => MoveCursorAsync(s, Directions.Up) },
            { Operations.MoveCursorDown, s => MoveCursorAsync(s, Directions.Down) },
            { Operations.MoveCursorLeft, s => MoveCursorAsync(s, Directions.Left) },
            { Operations.MoveCursorRight, s => MoveCursorAsync(s, Directions.Right) },
            { Operations.ToggleSelectionCursorEntry, s => ToggleSelectionCursorEntryAsync(s, true) },
            { Operations.OpenEntry, OpenEntryAsync },
            { Operations.OpenEntryByEditor1, s => OpenEntryByEditorAsync(s, 1) },
            { Operations.OpenEntryByEditor2, s => OpenEntryByEditorAsync(s, 2) },
            { Operations.OpenEntryByApp, OpenEntryByAppAsync },
            { Operations.JumpToParentFolder, JumpToParentFolderAsync },
            { Operations.JumpToRootFolder, JumpToRootFolderAsync },
            { Operations.CopyEntry, CopyEntryAsync },
            { Operations.DeleteEntry, DeleteEntryAsync },
            { Operations.EmptyTrashCan, EmptyTrashCanAsync },
            { Operations.OpenTrashCan, OpenTrashCanAsync },
            { Operations.MoveEntry, MoveEntryAsync },
        };
    }

    private async ValueTask SelectSortModeAndOrderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;

        var result = await WindowOperator.SelectSortModeAndOrderAsync(Dic, receiver.Owner);
        if (result.Result != DialogResultTypes.Ok)
            return;

        receiver.Folder.SetSortModeAndOrder(result.SortMode, result.SortOrder);
    }

    private async ValueTask JumpFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;
        var currentFolderPath = receiver.Owner.ViewModel.Model.Path;

        var result = await WindowOperator.JumpFolderAsync(Dic, receiver.Owner, currentFolderPath);
        if (result.Result != DialogResultTypes.Ok)
            return;

        if (await CheckIsAccessibleAsync(result.Path, receiver.Messenger) == false)
            return;

        receiver.Folder.Path = result.Path;
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

        if (target.IsFolder)
        {
            if (await CheckIsAccessibleAsync(target.Path, receiver.Messenger) == false)
                return;

            receiver.Folder.Path = target.Path;
        }
        else
        {
            if (await CheckIsAccessibleAsync(target.Path, receiver.Messenger) == false)
                return;

            await WindowOperator.EntryDisplay(Dic, receiver.Owner, target);
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

    private ValueTask CopyEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;
        
        var worker = Dic.GetInstance<ConfirmedFileSystemCopier, (InteractionMessenger, int)>((receiver.Messenger, 0));

        return CopyOrMoveEntryAsync(
            CopyOrMove.Copy,
            worker,
            (targetEntries, destFolder) => worker.Invoke(targetEntries, destFolder),
            shortcutKeyReceiver);
    }

    private async ValueTask DeleteEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;
        if (receiver.TargetEntries.Length == 0)
            return;

        var stats = Dic.GetInstance<EntriesStats, Entry[]>(receiver.TargetEntries);

        var result = await WindowOperator.EntryDeleteAsync(Dic, receiver.Owner, receiver.TargetEntries, stats);
        if (result.Result != DialogResultTypes.Yes)
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
            () => worker.Invoke(targetEntries, result.Mode)
        ));

        await receiver.BackgroundWorker.PushOperatorAsync(@operator);
    }

    private ValueTask MoveEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        throw new NotImplementedException();

#if false
        var receiver = (IFolderPanelShortcutKeyReceiver)shortcutKeyReceiver;
        
        var worker = Dic.GetInstance<ConfirmedFileSystemMover, (InteractionMessenger, int)>((receiver.Messenger, 0));

        return CopyOrMoveEntryAsync(
            CopyOrMove.Move,
            worker,
            (targetEntries, destFolder) => worker.Invoke(targetEntries, destFolder),
            shortcutKeyReceiver);
#endif
    }

    private ValueTask EmptyTrashCanAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        Dic.GetInstance<ITrashCanService>().EmptyTrashCan();
        return ValueTask.CompletedTask;
    }

    private ValueTask OpenTrashCanAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        Dic.GetInstance<ITrashCanService>().OpenTrashCan();
        return ValueTask.CompletedTask;
    }
}
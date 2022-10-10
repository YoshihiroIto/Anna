using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Gui.Views.Windows;
using Anna.UseCase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Anna.Gui.ShortcutKey;

public class FolderPanelShortcutKey : ShortcutKeyBase
{
    private readonly IServiceProviderContainer _dic;
    
    public FolderPanelShortcutKey(
        IServiceProviderContainer dic,
        IFolderServiceUseCase folderService,
        AppConfig appConfig,
        KeyConfig keyConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(folderService, appConfig, keyConfig, logger, objectLifetimeChecker)
    {
        _dic = dic;
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
        };
    }

    private async ValueTask SelectSortModeAndOrderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IFolderPanelShortcutKeyReceiver ?? throw new InvalidOperationException();

        var result = await WindowOperator.SelectSortModeAndOrderAsync(_dic, r.Owner);
        if (result.IsCancel)
            return;

        r.Folder.SetSortModeAndOrder(result.SortMode, result.SortOrder);
    }

    private async ValueTask JumpFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IFolderPanelShortcutKeyReceiver ?? throw new InvalidOperationException();

        var result = await WindowOperator.JumpFolderAsync(_dic, r.Owner);
        if (result.IsCancel)
            return;

        if (result.Path == "")
            return;

        if (await CheckIsAccessibleAsync(result.Path, r.Messenger) == false)
            return;

        r.Folder.Path = result.Path;
    }

    private static ValueTask MoveCursorAsync(IShortcutKeyReceiver shortcutKeyReceiver, Directions dir)
    {
        var r = shortcutKeyReceiver as IFolderPanelShortcutKeyReceiver ?? throw new InvalidOperationException();

        r.MoveCursor(dir);
        return ValueTask.CompletedTask;
    }

    private static ValueTask ToggleSelectionCursorEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver, bool isMoveDown)
    {
        var r = shortcutKeyReceiver as IFolderPanelShortcutKeyReceiver ?? throw new InvalidOperationException();

        r.ToggleSelectionCursorEntry(isMoveDown);
        return ValueTask.CompletedTask;
    }

    private async ValueTask OpenEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IFolderPanelShortcutKeyReceiver ?? throw new InvalidOperationException();

        var target = r.CurrentEntry;

        if (target.IsFolder)
        {
            if (await CheckIsAccessibleAsync(target.Path, r.Messenger) == false)
                return;

            r.Folder.Path = target.Path;
        }
        else
        {
            if (await CheckIsAccessibleAsync(target.Path, r.Messenger) == false)
                return;

            await WindowOperator.EntryDisplay(_dic, r.Owner, target);
        }
    }

    private ValueTask OpenEntryByEditorAsync(IShortcutKeyReceiver shortcutKeyReceiver, int index)
    {
        var r = shortcutKeyReceiver as IFolderPanelShortcutKeyReceiver ?? throw new InvalidOperationException();

        return r.CurrentEntry.IsFolder
            ? ValueTask.CompletedTask
            : OpenFileByEditorAsync(index, r.CurrentEntry.Path, r.Messenger);
    }

    private ValueTask OpenEntryByAppAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IFolderPanelShortcutKeyReceiver ?? throw new InvalidOperationException();

        return StartAssociatedAppAsync(r.CurrentEntry.Path, r.Messenger);
    }

    private async ValueTask JumpToParentFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IFolderPanelShortcutKeyReceiver ?? throw new InvalidOperationException();

        var parentDir = new DirectoryInfo(r.Folder.Path).Parent?.FullName;
        if (parentDir is null)
            return;

        if (await CheckIsAccessibleAsync(r.Folder.Path, r.Messenger) == false)
            return;

        r.Folder.Path = parentDir;
    }

    private async ValueTask JumpToRootFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var r = shortcutKeyReceiver as IFolderPanelShortcutKeyReceiver ?? throw new InvalidOperationException();

        var rootDir = Path.GetPathRoot(r.Folder.Path);
        if (rootDir is null)
            return;

        if (await CheckIsAccessibleAsync(rootDir, r.Messenger) == false)
            return;

        r.Folder.Path = rootDir;
    }
}
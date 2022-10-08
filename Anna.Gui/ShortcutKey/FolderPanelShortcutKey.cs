using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.UseCase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Anna.Gui.ShortcutKey;

public class FolderPanelShortcutKey : ShortcutKeyBase
{
    public FolderPanelShortcutKey(
        IServiceProviderContainer dic,
        IFolderServiceUseCase folderService,
        AppConfig appConfig,
        KeyConfig keyConfig,
        ILoggerUseCase logger)
        : base(folderService, keyConfig, logger)
    {
        _dic = dic;
        _appConfig = appConfig;
    }
    
    private readonly IServiceProviderContainer _dic;
    private readonly AppConfig _appConfig;

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
            {
                Operations.ToggleSelectionCursorEntry,
                s => ToggleSelectionCursorEntryAsync(s, true)
            },
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
        var result = await DialogOperator.SelectSortModeAndOrderAsync(_dic, shortcutKeyReceiver.Owner);
        if (result.IsCancel)
            return;

        shortcutKeyReceiver.Folder.SetSortModeAndOrder(result.SortMode, result.SortOrder);
    }

    private async ValueTask JumpFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var result = await DialogOperator.JumpFolderAsync(_dic, shortcutKeyReceiver.Owner);
        if (result.IsCancel)
            return;

        if (result.Path == "")
            return;

        if (await CheckIsAccessibleAsync(result.Path, shortcutKeyReceiver.Messenger) == false)
            return;

        shortcutKeyReceiver.Folder.Path = result.Path;
    }

    private static ValueTask MoveCursorAsync(IShortcutKeyReceiver shortcutKeyReceiver, Directions dir)
    {
        shortcutKeyReceiver.FolderPanelViewModel.MoveCursor(dir);
        return ValueTask.CompletedTask;
    }

    private static ValueTask ToggleSelectionCursorEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver, bool isMoveDown)
    {
        shortcutKeyReceiver.FolderPanelViewModel.ToggleSelectionCursorEntry(isMoveDown);
        return ValueTask.CompletedTask;
    }

    private async ValueTask OpenEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var target = shortcutKeyReceiver.CurrentEntry;

        if (target.IsFolder)
        {
            if (await CheckIsAccessibleAsync(target.Path, shortcutKeyReceiver.Messenger) == false)
                return;

            shortcutKeyReceiver.Folder.Path = target.Path;
        }
        else
        {
            if (await CheckIsAccessibleAsync(target.Path, shortcutKeyReceiver.Messenger) == false)
                return;

            await DialogOperator.EntryDisplay(_dic, shortcutKeyReceiver.Owner, target);
        }
    }

    private ValueTask OpenEntryByEditorAsync(IShortcutKeyReceiver shortcutKeyReceiver, int index)
    {
        var target = shortcutKeyReceiver.CurrentEntry;
        if (target.IsFolder)
            return ValueTask.CompletedTask;

        Task.Run(() =>
        {
            var editor = _appConfig.Data.FindEditor(index);
            var arguments = ProcessHelper.MakeEditorArguments(editor.Options, target.Path, 1);

            ProcessHelper.Execute(editor.Editor, arguments);
        });

        return ValueTask.CompletedTask;
    }

    private ValueTask OpenEntryByAppAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var targetPath = shortcutKeyReceiver.CurrentEntry.Path;

        Task.Run(() => ProcessHelper.RunAssociatedApp(targetPath));

        return ValueTask.CompletedTask;
    }

    private async ValueTask JumpToParentFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var parentDir = new DirectoryInfo(shortcutKeyReceiver.Folder.Path).Parent?.FullName;
        if (parentDir is null)
            return;

        if (await CheckIsAccessibleAsync(shortcutKeyReceiver.Folder.Path, shortcutKeyReceiver.Messenger) == false)
            return;

        shortcutKeyReceiver.Folder.Path = parentDir;
    }

    private async ValueTask JumpToRootFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var rootDir = Path.GetPathRoot(shortcutKeyReceiver.Folder.Path);
        if (rootDir is null)
            return;

        if (await CheckIsAccessibleAsync(rootDir, shortcutKeyReceiver.Messenger) == false)
            return;

        shortcutKeyReceiver.Folder.Path = rootDir;
    }
    
}
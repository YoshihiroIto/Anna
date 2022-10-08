using Anna.Constants;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace Anna.Gui.ShortcutKey;

public partial class ShortcutKeyManager
{
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

        if (await CheckIsAccessibleAsync(result.Path, shortcutKeyReceiver) == false)
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
            if (await CheckIsAccessibleAsync(target.Path, shortcutKeyReceiver) == false)
                return;

            shortcutKeyReceiver.Folder.Path = target.Path;
        }
        else
        {
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

        if (await CheckIsAccessibleAsync(shortcutKeyReceiver.Folder.Path, shortcutKeyReceiver) == false)
            return;

        shortcutKeyReceiver.Folder.Path = parentDir;
    }

    private async ValueTask JumpToRootFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var rootDir = Path.GetPathRoot(shortcutKeyReceiver.Folder.Path);
        if (rootDir is null)
            return;

        if (await CheckIsAccessibleAsync(rootDir, shortcutKeyReceiver) == false)
            return;

        shortcutKeyReceiver.Folder.Path = rootDir;
    }
}
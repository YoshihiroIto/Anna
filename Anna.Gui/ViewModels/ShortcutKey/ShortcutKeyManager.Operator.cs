using Anna.Constants;
using Anna.Gui.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.ShortcutKey;

public partial class ShortcutKeyManager
{
    private async ValueTask SelectSortModeAndOrderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var result = await DialogOperator.SelectSortModeAndOrderAsync(_dic, shortcutKeyReceiver.Owner);

        if (result.IsCancel)
            return;

        shortcutKeyReceiver.Folder.SetSortModeAndOrder(result.SortMode, result.SortOrder);
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

    private static ValueTask OpenEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        return shortcutKeyReceiver.FolderPanelViewModel.OpenCursorEntryAsync();
    }

    private static ValueTask JumpToParentFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var parentDir = new DirectoryInfo(shortcutKeyReceiver.FolderPanelViewModel.Model.Path).Parent?.FullName;

        return parentDir is not null
            ? shortcutKeyReceiver.FolderPanelViewModel.JumpToFolderAsync(parentDir)
            : ValueTask.CompletedTask;
    }

    private static ValueTask JumpToRootFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var rootDir = Path.GetPathRoot(shortcutKeyReceiver.FolderPanelViewModel.Model.Path);

        if (rootDir is not null)
            shortcutKeyReceiver.FolderPanelViewModel.JumpToFolderAsync(rootDir);

        return ValueTask.CompletedTask;
    }
}
using Anna.Constants;
using Anna.Gui.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.ShortcutKey;

public partial class ShortcutKeyManager
{
    private async ValueTask SelectSortModeAndOrderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var result = await DialogOperator.SelectSortModeAndOrderAsync(_dic, shortcutKeyReceiver);

        if (result.IsCancel)
            return;

        shortcutKeyReceiver.Directory.SetSortModeAndOrder(result.SortMode, result.SortOrder);
    }

    private static ValueTask MoveCursorAsync(IShortcutKeyReceiver shortcutKeyReceiver, Directions dir)
    {
        shortcutKeyReceiver.DirectoryPanelViewModel.MoveCursor(dir);
        return ValueTask.CompletedTask;
    }

    private static ValueTask ToggleSelectionCursorEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver, bool isMoveDown)
    {
        shortcutKeyReceiver.DirectoryPanelViewModel.ToggleSelectionCursorEntry(isMoveDown);
        return ValueTask.CompletedTask;
    }

    private static ValueTask OpenEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        shortcutKeyReceiver.DirectoryPanelViewModel.OpenCursorEntry();
        return ValueTask.CompletedTask;
    }

    private static ValueTask JumpToParentDirectoryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var parentDir = new DirectoryInfo(shortcutKeyReceiver.DirectoryPanelViewModel.Model.Path).Parent?.FullName;

        if (parentDir is not null)
            shortcutKeyReceiver.DirectoryPanelViewModel.JumpToDirectory(parentDir);
        
        return ValueTask.CompletedTask;
    }
    
    private static ValueTask JumpToRootDirectoryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var rootDir = System.IO.Path.GetPathRoot(shortcutKeyReceiver.DirectoryPanelViewModel.Model.Path);

        if (rootDir is not null)
            shortcutKeyReceiver.DirectoryPanelViewModel.JumpToDirectory(rootDir);

        return ValueTask.CompletedTask;
    }
}
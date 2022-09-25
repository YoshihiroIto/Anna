using Anna.Constants;
using Anna.Gui.Interfaces;
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
        shortcutKeyReceiver.DirectoryViewViewModel.MoveCursor(dir);
        return ValueTask.CompletedTask;
    }

    private static ValueTask ToggleSelectionCursorEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver, bool isMoveDown)
    {
        shortcutKeyReceiver.DirectoryViewViewModel.ToggleSelectionCursorEntry(isMoveDown);
        return ValueTask.CompletedTask;
    }
    
    private static ValueTask OpenEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        shortcutKeyReceiver.DirectoryViewViewModel.OpenCursorEntry();
        return ValueTask.CompletedTask;
    }
}
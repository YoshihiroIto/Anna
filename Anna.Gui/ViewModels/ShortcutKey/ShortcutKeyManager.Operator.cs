using Anna.Constants;
using Anna.Gui.Interfaces;
using Anna.Gui.ViewModels.Messaging;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Gui.Views.Panels;
using Anna.Strings;
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

    private async ValueTask JumpFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var result = await DialogOperator.JumpFolderAsync(_dic, shortcutKeyReceiver.Owner);
        if (result.IsCancel)
            return;
        
        if (string.IsNullOrEmpty(result.Path))
            return;

        if (CheckIsAccessible(result.Path, shortcutKeyReceiver) == false)
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

    private static ValueTask OpenEntryAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        return shortcutKeyReceiver.FolderPanelViewModel.OpenCursorEntryAsync();
    }

    private ValueTask JumpToParentFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var parentDir = new DirectoryInfo(shortcutKeyReceiver.Folder.Path).Parent?.FullName;
        if (parentDir is null)
            return ValueTask.CompletedTask;
        
        if (CheckIsAccessible(shortcutKeyReceiver.Folder.Path, shortcutKeyReceiver) == false)
            return ValueTask.CompletedTask;

        shortcutKeyReceiver.Folder.Path = parentDir;

        return ValueTask.CompletedTask;
    }

    private ValueTask JumpToRootFolderAsync(IShortcutKeyReceiver shortcutKeyReceiver)
    {
        var rootDir = Path.GetPathRoot(shortcutKeyReceiver.Folder.Path);
        if (rootDir is null)
            return ValueTask.CompletedTask;
        
        if (CheckIsAccessible(rootDir, shortcutKeyReceiver) == false)
            return ValueTask.CompletedTask;

        shortcutKeyReceiver.Folder.Path = rootDir;

        return ValueTask.CompletedTask;
    }

    private bool CheckIsAccessible(string path, IShortcutKeyReceiver shortcutKeyReceiver)
    {
        if (_folderService.IsAccessible(path))
            return true;

        shortcutKeyReceiver.FolderPanelViewModel.Messenger.Raise(
            new InformationMessage(
                Resources.AppName,
                string.Format(Resources.Message_AccessDenied, path),
                DialogViewModel.MessageKeyInformation));

        return false;
    }
}
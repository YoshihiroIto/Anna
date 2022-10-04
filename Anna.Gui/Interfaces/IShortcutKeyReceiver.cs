using Anna.DomainModel;
using Anna.Gui.Views.Panels;
using Avalonia.Controls;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.Interfaces;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
    Folder Folder { get; }
    FolderPanelViewModel FolderPanelViewModel { get; }

    Entry[] CollectTargetEntries();
}
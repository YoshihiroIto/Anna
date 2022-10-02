using Anna.Gui.Views.Panels;
using Avalonia.Controls;
using Directory=Anna.DomainModel.Directory;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.Interfaces;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
    Directory Directory { get; }
    DirectoryPanelViewModel DirectoryPanelViewModel { get; }

    Entry[] CollectTargetEntries();
}
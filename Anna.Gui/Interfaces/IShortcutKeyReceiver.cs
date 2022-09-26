using Anna.DomainModel;
using Anna.Gui.Views;
using Anna.Gui.Views.Panels;
using Avalonia.Controls;
using Directory=Anna.DomainModel.Directory;

namespace Anna.Gui.Interfaces;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
    Directory Directory { get; }
    DirectoryPanelViewModel DirectoryPanelViewModel { get; }

    Entry[] CollectTargetEntries();
}
using Anna.DomainModel;
using Anna.Gui.Views;
using Avalonia.Controls;
using Directory=Anna.DomainModel.Directory;

namespace Anna.Gui.Interfaces;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
    Directory Directory { get; }
    DirectoryViewViewModel DirectoryViewViewModel { get; }

    Entry[] CollectTargetEntries();
}
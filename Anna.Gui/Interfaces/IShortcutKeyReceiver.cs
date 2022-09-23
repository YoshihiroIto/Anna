using Anna.DomainModel;
using Avalonia.Controls;
using Directory=Anna.DomainModel.Directory;

namespace Anna.Gui.Interfaces;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
    Directory Directory { get; }

    Entry[] CollectTargetEntries();
}
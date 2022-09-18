using Anna.DomainModel;
using Avalonia.Controls;
using Directory=Anna.DomainModel.Directory;

namespace Anna.UseCase.Interfaces;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
    Directory Directory { get; }

    Entry[] CollectTargetEntities();
}
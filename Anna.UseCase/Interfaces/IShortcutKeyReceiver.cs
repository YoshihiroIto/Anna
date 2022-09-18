using Avalonia.Controls;

namespace Anna.UseCase.Interfaces;

public interface IShortcutKeyReceiver
{
    Window Owner { get; }
}
using Anna.UseCase;
using Avalonia.Controls;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Anna.ServiceProvider")]

namespace Anna.Gui.Views.Dialogs.Base;

public class DialogBase : Window
{
    protected internal ILoggerUseCase Logger { get; set; } = null!;

    public DialogBase()
    {
        Loaded += (_, _) => Logger.Start(GetType().Name);
        Closed += (_, _) => Logger.End(GetType().Name);
    }
}
using Anna.UseCase;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Anna.ServiceProvider")]

namespace Anna.Gui.Views.Dialogs.Base;

public class DialogBase<T> : DialogBase
    where T : DialogViewModel
{
    protected T ViewModel => DataContext as T ?? throw new NullReferenceException();
}

public class DialogBase : Window
{
    protected internal ILoggerUseCase Logger { get; set; } = null!;


    public DialogBase()
    {
        Loaded += (_, _) => Logger.Start(GetType().Name);
        Closed += (_, _) => Logger.End(GetType().Name);
    }

    protected bool DoMoveFocus(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down or Key.Right:
                MoveFocus(true);
                e.Handled = true;
                return true;

            case Key.Up or Key.Left:
                MoveFocus(false);
                e.Handled = true;
                return true;
        }

        return false;
    }

    private static void MoveFocus(bool isNext)
    {
        var current = FocusManager.Instance?.Current;
        if (current is null)
            return;

        var next = KeyboardNavigationHandler.GetNext(current,
            isNext
                ? NavigationDirection.Next
                : NavigationDirection.Previous);

        if (next is not null)
            FocusManager.Instance?.Focus(next, NavigationMethod.Directional);
    }
}
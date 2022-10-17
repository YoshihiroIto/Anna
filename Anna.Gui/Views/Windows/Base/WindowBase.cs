using Anna.Service.Services;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Anna.ServiceProvider")]

namespace Anna.Gui.Views.Windows.Base;

public class WindowBase<T> : WindowBase
    where T : WindowBaseViewModel
{
    public new T ViewModel => (T)_ViewModel! ?? throw new InvalidOperationException();
}

public class WindowBase : Window
{
    public WindowBaseViewModel ViewModel => _ViewModel ?? throw new InvalidOperationException();
    protected WindowBaseViewModel? _ViewModel;
    
    protected internal ILoggerService Logger { get; set; } = null!;

    public WindowBase()
    {
        Loaded += (_, _) => Logger.Start(GetType().Name);
        Closed += (_, _) => Logger.End(GetType().Name);
        
        PropertyChanged += (_, e) =>
        {
            if (e.Property == DataContextProperty)
                _ViewModel = DataContext as WindowBaseViewModel ?? throw new NotSupportedException();
        };
    }

    protected static bool DoMoveFocus(KeyEventArgs e)
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
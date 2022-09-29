using Anna.Gui.ViewModels.Messaging;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using System;

namespace Anna.Gui.Views.Behaviors.Messaging;

public class WindowInteractionMessageAction: AvaloniaObject, IAction
{
    public object? Execute(object? sender, object? parameter)
    {
        if (parameter is not WindowActionMessage message)
            return null;

        if (sender is not Trigger { AssociatedObject: Avalonia.Controls.Window window })
            return null;

        switch (message.Action)
        {
            case WindowAction.Close:
                window.Close();
                break;

            case WindowAction.Maximize:
                break;

            case WindowAction.Minimize:
                break;

            case WindowAction.Normal:
                break;

            case WindowAction.Active:
                window.Activate();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }
}
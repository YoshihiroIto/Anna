using Anna.Gui.ViewModels.Messaging;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Behaviors.Messaging;

public class WindowInteractionMessageAction : AvaloniaObject, IAction, IAsyncAction
{
    public object Execute(object? sender, object? parameter)
    {
        throw new NotSupportedException();
    }

    public ValueTask ExecuteAsync(Trigger sender, InteractionMessage message)
    {
        if (message is not WindowActionMessage windowActionMessage)
            return ValueTask.CompletedTask;

        if (sender is not { AssociatedObject: Avalonia.Controls.Window window })
            return ValueTask.CompletedTask;

        switch (windowActionMessage.Action)
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

        return ValueTask.CompletedTask;
    }
}
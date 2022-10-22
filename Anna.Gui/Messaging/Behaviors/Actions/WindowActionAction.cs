using Anna.Gui.Messaging.Messages;
using Anna.Service;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging.Behaviors.Actions;

public sealed class WindowActionAction : AvaloniaObject, IAction, IAsyncAction
{
    public object Execute(object? sender, object? parameter)
    {
        throw new NotSupportedException();
    }

    public ValueTask ExecuteAsync(
        Trigger sender,
        InteractionMessage message,
        IHasServiceProviderContainer hasServiceProviderContainer)
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
                throw new NotImplementedException();

            case WindowAction.Minimize:
                throw new NotImplementedException();

            case WindowAction.Normal:
                throw new NotImplementedException();

            case WindowAction.Active:
                window.Activate();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return ValueTask.CompletedTask;
    }
}
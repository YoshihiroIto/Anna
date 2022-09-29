using Anna.Gui.Foundations;
using Anna.Gui.ViewModels.Messaging;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Behaviors.Messaging;

public class InformationDialogInteractionMessageAction : AvaloniaObject, IAction, IAsyncAction
{
    public object Execute(object? sender, object? parameter)
    {
        throw new NotSupportedException();
    }

    public async ValueTask ExecuteAsync(Trigger sender, InteractionMessage message)
    {
        if (message is not InformationMessage informationMessage)
            return;

        if (sender is not { AssociatedObject: IControl control })
            return;

        var owner = ControlHelper.FindOwnerWindow(control);

        await DialogOperator.DisplayInformationAsync(
            message.ServiceProviderContainer ?? throw new NullReferenceException(),
            owner,
            informationMessage.Title,
            informationMessage.Text);
    }
}
using Anna.Gui.Foundations;
using Anna.Gui.Views.Windows;
using Anna.UseCase;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging.Behaviors;

public class InformationDialogInteractionMessageAction : AvaloniaObject, IAction, IAsyncAction
{
    public object Execute(object? sender, object? parameter)
    {
        throw new NotSupportedException();
    }

    public async ValueTask ExecuteAsync(
        Trigger sender,
        InteractionMessage message,
        IHasServiceProviderContainer hasServiceProviderContainer)
    {
        if (message is not InformationMessage informationMessage)
            return;

        if (sender is not { AssociatedObject: IControl control })
            return;

        var owner = ControlHelper.FindOwnerWindow(control);

        informationMessage.Response = await WindowOperator.DisplayInformationAsync(
            hasServiceProviderContainer.Dic,
            owner,
            informationMessage.Title,
            informationMessage.Text);
    }
}
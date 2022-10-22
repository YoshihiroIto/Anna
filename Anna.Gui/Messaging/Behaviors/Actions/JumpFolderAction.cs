using Anna.Gui.Foundations;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows;
using Anna.Service;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging.Behaviors.Actions;

public sealed class JumpFolderAction : AvaloniaObject, IAction, IAsyncAction
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
        if (message is not JumpFolderMessage jumpFolderMessage)
            return;

        if (sender is not { AssociatedObject: IControl control })
            return;

        var owner = ControlHelper.FindOwnerWindow(control);

        var result = await WindowOperator.JumpFolderAsync(
            hasServiceProviderContainer.Dic,
            owner,
            jumpFolderMessage.CurrentFolderPath);

        jumpFolderMessage.Response = result;
    }
}
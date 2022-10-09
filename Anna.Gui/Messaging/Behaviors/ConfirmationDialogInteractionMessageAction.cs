﻿using Anna.Gui.Foundations;
using Anna.UseCase;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging.Behaviors;

public class ConfirmationDialogInteractionMessageAction : AvaloniaObject, IAction, IAsyncAction
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
        if (message is not ConfirmationMessage confirmationMessage)
            return;

        if (sender is not { AssociatedObject: IControl control })
            return;

        var owner = ControlHelper.FindOwnerWindow(control);

        confirmationMessage.Response = await DialogOperator.DisplayConfirmationAsync(
            hasServiceProviderContainer.ServiceProviderContainer,
            owner,
            confirmationMessage.Title,
            confirmationMessage.Text,
            confirmationMessage.ConfirmationType);
    }
}
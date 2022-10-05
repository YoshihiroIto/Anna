﻿using Anna.Gui.Foundations;
using Anna.Gui.ViewModels.Messaging;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Behaviors.Messaging;

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

        message.Response = await DialogOperator.DisplayConfirmationAsync(
            hasServiceProviderContainer.ServiceProviderContainer,
            owner,
            confirmationMessage.Title,
            confirmationMessage.Text,
            confirmationMessage.ConfirmationType);
    }
}
using Anna.Gui.Interactions.Hotkey;
using Anna.Gui.Interfaces;
using Anna.Gui.Messaging;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;
using System.Diagnostics;

namespace Anna.Gui.Views.Behaviors;

public sealed class ViewerBehavior : Behavior<UserControl>
{
    private Messenger ParentMessenger => HotkeyReceiver.Messenger;

    private IHotkeyReceiver HotkeyReceiver
    {
        get
        {
            _ = AssociatedObject ?? throw new NullReferenceException();

            return (IHotkeyReceiver)AssociatedObject ?? throw new NullReferenceException();
        }
    }

    protected override void OnAttached()
    {
        Debug.Assert(AssociatedObject is IHotkeyReceiver);

        base.OnAttached();

        AssociatedObject?.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);

        base.OnDetaching();
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var viewModel = AssociatedObject?.DataContext as IViewerViewModel ?? throw new NullReferenceException();

        if (e.Key == Key.Escape)
        {
            await ParentMessenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));
            e.Handled = true;
        }
        else
        {
            await viewModel.Hotkey.OnKeyDownAsync(HotkeyReceiver, e);
        }
    }
}
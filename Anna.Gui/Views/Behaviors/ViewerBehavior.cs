using Anna.Gui.Interactions.Hotkey;
using Anna.Gui.Interfaces;
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
    private IViewerHotkeyReceiver HotkeyReceiver
    {
        get
        {
            _ = AssociatedObject ?? throw new NullReferenceException();

            return (IViewerHotkeyReceiver)AssociatedObject ?? throw new NullReferenceException();
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
        if (e.Key == Key.Escape)
        {
            await HotkeyReceiver.Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close));
            e.Handled = true;
        }
        else
        {
            var viewModel = AssociatedObject?.DataContext as IViewerViewModel ?? throw new NullReferenceException();
            await viewModel.Hotkey.OnKeyDownAsync(HotkeyReceiver, e);
        }
    }
}
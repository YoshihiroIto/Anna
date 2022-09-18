using Anna.ViewModels;
using Anna.Views.ShortcutKey;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;

namespace Anna.Views.Behaviors;

public class DirectoryViewInputBehavior : Behavior<DirectoryView>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
            return;

        AssociatedObject.Focusable = true;
        AssociatedObject.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, OnPreviewKeyDown);

        base.OnDetaching();
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        var manager = _shortcutKeyManager ??=
            (AssociatedObject?.DataContext as DirectoryViewViewModel)?.ShortcutKeyManager;

        manager?.OnKeyDown(AssociatedObject ?? throw new NullReferenceException(), e);
    }

    private ShortcutKeyManager? _shortcutKeyManager;
}
using Anna.Gui.ViewModels;
using Anna.Gui.ViewModels.ShortcutKey;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Behaviors;

public class DirectoryViewInputBehavior : Behavior<Gui.Views.DirectoryView>
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

    private ValueTask OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        var manager = _shortcutKeyManager ??=
            (AssociatedObject?.DataContext as DirectoryViewViewModel)?.ShortcutKeyManager;

        return manager?.OnKeyDown(AssociatedObject ?? throw new NullReferenceException(), e) ?? ValueTask.CompletedTask;
    }

    private ShortcutKeyManager? _shortcutKeyManager;
}
using Anna.Gui.ViewModels;
using Anna.Gui.ViewModels.ShortcutKey;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Behaviors;

public class DirectoryViewInputBehavior : Behavior<DirectoryView>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
            AssociatedObject.Focusable = true;
        }
    }
    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var parent = AssociatedObject?.Parent;

        while (parent is not null)
        {
            if (parent is Window parentWindow)
            {
                _parentWindow = parentWindow;
                break;
            }

            parent = parent.Parent;
        }

        _parentWindow?.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
            AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;

        _parentWindow?.RemoveHandler(InputElement.KeyDownEvent, OnPreviewKeyDown);

        base.OnDetaching();
    }

    private ValueTask OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (FocusManager.Instance?.Current is MenuItem)
            return ValueTask.CompletedTask;

        var manager = _shortcutKeyManager ??=
            (AssociatedObject?.DataContext as DirectoryViewViewModel)?.ShortcutKeyManager;

        return manager?.OnKeyDown(AssociatedObject ?? throw new NullReferenceException(), e) ?? ValueTask.CompletedTask;
    }

    private Window? _parentWindow;
    private ShortcutKeyManager? _shortcutKeyManager;
}
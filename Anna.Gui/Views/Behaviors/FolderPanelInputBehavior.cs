using Anna.Gui.Foundations;
using Anna.Gui.Views.Panels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;
using ShortcutKeyManager=Anna.Gui.ShortcutKey.ShortcutKeyManager;

namespace Anna.Gui.Views.Behaviors;

public class FolderPanelInputBehavior : Behavior<FolderPanel>
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

    private async ValueTask OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (FocusManager.Instance?.Current is MenuItem)
            return;

        var viewModel = AssociatedObject?.DataContext as FolderPanelViewModel;

        if (viewModel is not null)
        {
            await DispatcherHelper.DoEventsAsync();

            if (viewModel.Model.IsInEntriesUpdating)
                return;
        }

        var manager = _shortcutKeyManager ??= viewModel?.ShortcutKeyManager;
        
        if (manager is not null)
            await manager.OnKeyDownAsync(AssociatedObject ?? throw new NullReferenceException(), e);
    }

    private Window? _parentWindow;
    private ShortcutKeyManager? _shortcutKeyManager;
}
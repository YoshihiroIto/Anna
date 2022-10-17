using Anna.Gui.Foundations;
using Anna.Gui.Views.Panels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Behaviors;

public sealed class FolderPanelInputBehavior : Behavior<FolderPanel>
{
    private Window? _parentWindow;
    
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

        var viewModel = AssociatedObject?.ViewModel;

        if (viewModel is not null)
        {
            await DispatcherHelper.DoEventsAsync();

            if (viewModel.Model.IsInEntriesUpdating)
                return;
        }

        var shortcutKey = viewModel?.ShortcutKey;
        
        if (shortcutKey is not null)
            await shortcutKey.OnKeyDownAsync(AssociatedObject ?? throw new NullReferenceException(), e);
    }
}
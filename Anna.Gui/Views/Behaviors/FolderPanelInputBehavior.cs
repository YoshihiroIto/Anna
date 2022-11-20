using Anna.Gui.Foundations;
using Anna.Gui.Views.Controls;
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
        _ = AssociatedObject ?? throw new NullReferenceException();

        _parentWindow = ControlHelper.FindOwnerWindow(AssociatedObject);

        _parentWindow?.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        _parentWindow?.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        _parentWindow?.AddHandler(DragDrop.DropEvent, OnDrop);
        _parentWindow?.AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
            AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;

        _parentWindow?.RemoveHandler(InputElement.KeyDownEvent, OnPreviewKeyDown);
        _parentWindow?.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        _parentWindow?.RemoveHandler(DragDrop.DropEvent, OnDrop);
        _parentWindow?.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);

        base.OnDetaching();
    }

    private async ValueTask OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (AssociatedObject is null)
            return;

        if (FocusManager.Instance?.Current is MenuItem)
            return;

        var viewModel = AssociatedObject.ViewModel;

        await DispatcherHelper.DoEventsAsync();

        if (viewModel.Model.IsInEntriesUpdating)
            return;

        await viewModel.Hotkey.OnKeyDownAsync(AssociatedObject, e);
    }

    private void OnPointerMoved(object? sender, EventArgs e)
    {
        if (AssociatedObject is null)
            return;
        
        var ownerWindow = ControlHelper.FindOwnerWindow(AssociatedObject);
        
        DragDrop.SetAllowDrop(ownerWindow, true);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (AssociatedObject is null)
            return;
        
        var ownerWindow = ControlHelper.FindOwnerWindow(AssociatedObject);

        if (e.Data.Get(DropDataFormat.FolderPanel) is FolderPanel dropSource)
        {
            if (ReferenceEquals(AssociatedObject, dropSource))
            {
                DragDrop.SetAllowDrop(ownerWindow, false);
                return;
            }
        }
        
        DragDrop.SetAllowDrop(ownerWindow, e.Data.Contains(DataFormats.FileNames));
    }

    private async ValueTask OnDrop(object? sender, DragEventArgs e)
    {
        if (AssociatedObject is null)
            return;

        if (e.Data.Contains(DataFormats.FileNames) == false)
            return;

        var fileNames = e.Data.GetFileNames();
        if (fileNames is null)
            return;
        
        await AssociatedObject.ViewModel.Drop.OnFileDropAsync(AssociatedObject, fileNames);
    }
}
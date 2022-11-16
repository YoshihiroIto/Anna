﻿using Anna.Gui.Foundations;
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
        AssociatedObject?.AddHandler(DragDrop.DropEvent, OnDrop);
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
            AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;

        _parentWindow?.RemoveHandler(InputElement.KeyDownEvent, OnPreviewKeyDown);
        AssociatedObject?.RemoveHandler(DragDrop.DropEvent, OnDrop);

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

    private async ValueTask OnDrop(object? sender, DragEventArgs e)
    {
        if (AssociatedObject is null)
            return;

        if (e.Data.Contains(DataFormats.FileNames) == false)
            return;

        var fileNames = e.Data.GetFileNames();
        if (fileNames is null)
            return;
        
        var viewModel = AssociatedObject.ViewModel;
        await viewModel.Drop.OnFileDropAsync(AssociatedObject, fileNames);
    }
}
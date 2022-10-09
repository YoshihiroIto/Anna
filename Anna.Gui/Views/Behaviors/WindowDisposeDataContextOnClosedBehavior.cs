using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;

namespace Anna.Gui.Views.Behaviors;

public class WindowDisposeDataContextOnClosedBehavior : Behavior<Window>
{
    private bool _isDisposed;
    
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
            AssociatedObject.Closed += AssociatedObjectOnClosed;
    }

    protected override void OnDetaching()
    {
        DisposeAssociatedObject();
        
        if (AssociatedObject is not null)
            AssociatedObject.Closed -= AssociatedObjectOnClosed;

        base.OnDetaching();
    }

    private void AssociatedObjectOnClosed(object? sender, EventArgs eventArgs)
    {
        DisposeAssociatedObject();
    }

    private void DisposeAssociatedObject()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        (AssociatedObject?.DataContext as IDisposable)?.Dispose();
    }
}
using Anna.Views;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Anna.Foundations.Behaviors;

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
    }
}
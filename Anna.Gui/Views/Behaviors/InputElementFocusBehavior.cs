using Avalonia;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Anna.Gui.Views.Behaviors;

public class InputElementFocusBehavior : Behavior<InputElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
            AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
    }

    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (AssociatedObject is not null)
            AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;

        FocusManager.Instance?.Focus(AssociatedObject, NavigationMethod.Directional);
    }
}
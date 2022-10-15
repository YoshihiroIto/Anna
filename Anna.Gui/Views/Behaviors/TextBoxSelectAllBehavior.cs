using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace Anna.Gui.Views.Behaviors;

public sealed class TextBoxSelectAllBehavior : Behavior<TextBox>
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

        AssociatedObject?.SelectAll();
    }
}
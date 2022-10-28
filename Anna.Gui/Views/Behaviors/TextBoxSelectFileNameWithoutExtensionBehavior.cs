using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace Anna.Gui.Views.Behaviors;

public sealed class TextBoxSelectFileNameWithoutExtensionBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
            AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
    }

    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (AssociatedObject is null)
            return;

        AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;

        var text = AssociatedObject.Text;
        
        if (text is null)
            return;

        var dotPos = text.LastIndexOf('.');

        if (dotPos > 0)
        {
            AssociatedObject.SelectionStart = 0;
            AssociatedObject.SelectionEnd = dotPos;
        }
        else
            AssociatedObject.SelectAll();
    }
}
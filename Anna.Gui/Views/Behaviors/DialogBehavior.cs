using Anna.Constants;
using Anna.Gui.Views.Windows.Base;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Anna.Gui.Views.Behaviors;

public sealed class DialogBehavior : Behavior<Window>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
            AssociatedObject.KeyDown += AssociatedObjectOnKeyDown;
    }
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
            AssociatedObject.KeyDown -= AssociatedObjectOnKeyDown;

        base.OnDetaching();
    }

    private void AssociatedObjectOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (AssociatedObject is null)
            return;

        if (e.Key == Key.Escape)
        {
            if (AssociatedObject.DataContext is not WindowBaseViewModel viewModel)
                return;

            viewModel.DialogResult = DialogResultTypes.Cancel;
            AssociatedObject.Close();
        }
    }
}
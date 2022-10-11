using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using System;
using IAction=Avalonia.Xaml.Interactivity.IAction;

namespace Anna.Gui.Views.Behaviors;

public sealed class KeyDownTriggerBehavior : Trigger<Control>
{
    public static readonly StyledProperty<Key> KeyProperty =
        AvaloniaProperty.Register<KeyDownTriggerBehavior, Key>(nameof(Key));

    public static readonly StyledProperty<KeyModifiers> KeyModifiersProperty =
        AvaloniaProperty.Register<KeyDownTriggerBehavior, KeyModifiers>(nameof(KeyModifiers));

    public Key Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public KeyModifiers KeyModifiers
    {
        get => GetValue(KeyModifiersProperty);
        set => SetValue(KeyModifiersProperty, value);
    }

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

        if (e.Key != Key || e.KeyModifiers != KeyModifiers)
            return;

        foreach (var avaloniaObject in Actions)
        {
            if (avaloniaObject is not IAction action)
                throw new NotSupportedException();

            action.Execute(sender, EventArgs.Empty);
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;

namespace Anna.Gui.Views.Controls;

public sealed class EditableTextBlock : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<EditableTextBlock, string>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<EditableTextBlock, bool>(nameof(IsEditing));

    internal static readonly StyledProperty<IControlTemplate> EditTemplateProperty =
        AvaloniaProperty.Register<EditableTextBlock, IControlTemplate>(nameof(EditTemplate));

    internal static readonly StyledProperty<IControlTemplate> DisplayTemplateProperty =
        AvaloniaProperty.Register<EditableTextBlock, IControlTemplate>(nameof(DisplayTemplate));

    public event EventHandler<IsEditingChangedArgs>? IsEditingChanged;
    public event EventHandler<PointerPressedEventArgs>? PreviewPointerPress;

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    private IControlTemplate EditTemplate
    {
        get => GetValue(EditTemplateProperty);
        init => SetValue(EditTemplateProperty, value);
    }

    private IControlTemplate DisplayTemplate
    {
        get => GetValue(DisplayTemplateProperty);
        init => SetValue(DisplayTemplateProperty, value);
    }

    static EditableTextBlock()
    {
        IsEditingProperty.Changed.Subscribe(e => (e.Sender as EditableTextBlock)?.OnIsEditingChanged());
    }

    public EditableTextBlock()
    {
        Loaded += (_, _) => UpdateTemplate();

        AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);
    }
    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PreviewPointerPress?.Invoke(this, e);
    }

    private void OnIsEditingChanged()
    {
        UpdateTemplate();

        IsEditingChanged?.Invoke(this, new IsEditingChangedArgs(IsEditing));
    }
    private void UpdateTemplate()
    {
        Template = IsEditing ? EditTemplate : DisplayTemplate;
    }
}

public sealed class IsEditingChangedArgs : EventArgs
{
    public readonly bool IsEditing;

    public IsEditingChangedArgs(bool isEditing)
    {
        IsEditing = isEditing;
    }
}

internal sealed class EditableTextBlockTextBoxBehavior : Behavior<TextBox>
{
    private EditableTextBlock Parent =>
        AssociatedObject?.Parent as EditableTextBlock ?? throw new NullReferenceException();
    
    private string _currentText = "";
    
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
            AssociatedObject.KeyDown += AssociatedObjectOnKeyDown;
            AssociatedObject.LostFocus += AssociatedObjectOnLostFocus;
        }
    }
    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is not null)
        {
            AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;
            AssociatedObject.KeyDown -= AssociatedObjectOnKeyDown;
            AssociatedObject.LostFocus -= AssociatedObjectOnLostFocus;
        }
    }

    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _currentText = Parent.Text;
        AssociatedObject?.SelectAll();
    }

    private void AssociatedObjectOnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                Parent.IsEditing = false;
                e.Handled = true;
                break;

            case Key.Escape:
                Parent.Text = _currentText;
                Parent.IsEditing = false;
                e.Handled = true;
                break;
        }
    }

    private void AssociatedObjectOnLostFocus(object? sender, RoutedEventArgs e)
    {
        Parent.IsEditing = false;
    }
}
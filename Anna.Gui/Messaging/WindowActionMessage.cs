using Avalonia;

namespace Anna.Gui.Messaging;

public enum WindowAction
{
    Close,
    Maximize,
    Minimize,
    Normal,
    Active
}

public sealed class WindowActionMessage : InteractionMessage
{
    public static readonly StyledProperty<WindowAction> ActionProperty =
        AvaloniaProperty.Register<WindowActionMessage, WindowAction>(nameof(Action));

    public WindowAction Action
    {
        get => GetValue(ActionProperty);
        init => SetValue(ActionProperty, value);
    }

    public WindowActionMessage(
        WindowAction action,
        string messageKey)
        : base(messageKey)
    {
        Action = action;
    }
}
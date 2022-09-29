using Avalonia;

namespace Anna.Gui.ViewModels.Messaging;

public enum WindowAction
{
    Close,
    Maximize,
    Minimize,
    Normal,
    Active
}

public class WindowActionMessage : InteractionMessage
{
    public static readonly StyledProperty<WindowAction> ActionProperty =
        AvaloniaProperty.Register<WindowActionMessage, WindowAction>(nameof(Action));

    public WindowActionMessage(WindowAction action, string messageKey)
        : base(messageKey)
    {
        Action = action;
    }

    public WindowAction Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }
}
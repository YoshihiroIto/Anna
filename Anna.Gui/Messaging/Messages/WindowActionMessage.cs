namespace Anna.Gui.Messaging.Messages;

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
    public WindowAction Action { get; init; }

    public WindowActionMessage(WindowAction action, string messageKey)
        : base(messageKey)
    {
        Action = action;
    }
}
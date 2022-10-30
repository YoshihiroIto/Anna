namespace Anna.Gui.Messaging.Messages;

public enum WindowAction
{
    Close,
    Maximize,
    Minimize,
    Normal,
    Active
}

public sealed class WindowActionMessage : MessageBase
{
    public readonly WindowAction Action;

    public WindowActionMessage(WindowAction action, string messageKey)
        : base(messageKey)
    {
        Action = action;
    }
}
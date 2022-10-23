using Anna.Constants;

namespace Anna.Gui.Messaging.Messages;

public sealed class ConfirmationMessage : InteractionMessage
{
    public DialogResultTypes Response { get; internal set; }

    public readonly string Title;
    public readonly string Text;
    public readonly DialogResultTypes Confirmations;

    public ConfirmationMessage(
        string title,
        string text,
        DialogResultTypes confirmations,
        string messageKey)
        : base(messageKey)
    {
        Title = title;
        Text = text;
        Confirmations = confirmations;
    }
}
using Anna.Constants;

namespace Anna.Gui.Messaging.Messages;

public sealed class ConfirmationMessage : InteractionMessage
{
    public DialogResultTypes Response { get; internal set; }

    public readonly string Title;
    public readonly string Text;
    public readonly ConfirmationTypes ConfirmationType;

    public ConfirmationMessage(
        string title,
        string text,
        ConfirmationTypes confirmationTypes,
        string messageKey)
        : base(messageKey)
    {
        Title = title;
        Text = text;
        ConfirmationType = confirmationTypes;
    }
}
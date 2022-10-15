using Anna.Gui.Views.Windows.Base;

namespace Anna.Gui.Messaging.Messages;

public sealed class ConfirmationMessage : InteractionMessage
{
    public string Title { get; init; }

    public string Text { get; init; }

    public ConfirmationTypes ConfirmationType { get; init; }

    public DialogResultTypes Response { get; internal set; }

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
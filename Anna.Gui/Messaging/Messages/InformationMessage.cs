using Anna.Constants;

namespace Anna.Gui.Messaging.Messages;

public sealed class InformationMessage : InteractionMessage
{
    public DialogResultTypes Response { get; internal set; }

    public readonly string Title;
    public readonly string Text;

    public InformationMessage(
        string title,
        string text,
        string messageKey)
        : base(messageKey)
    {
        Title = title;
        Text = text;
    }
}
using Anna.Constants;

namespace Anna.Gui.Messaging.Messages;

public sealed class InformationMessage : InteractionMessage
{
    public string Title { get; init; }
    
    public string Text { get; init; }
    
    public DialogResultTypes Response { get; internal set; }

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
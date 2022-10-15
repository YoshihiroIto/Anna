namespace Anna.Gui.Messaging.Messages;

public class InteractionMessage
{
    public string MessageKey { get; init; }
    
    public InteractionMessage(string messageKey)
    {
        MessageKey = messageKey;
    }
}
namespace Anna.Gui.Messaging.Messages;

public class InteractionMessage
{
    public readonly string MessageKey;
    
    public InteractionMessage(string messageKey)
    {
        MessageKey = messageKey;
    }
}
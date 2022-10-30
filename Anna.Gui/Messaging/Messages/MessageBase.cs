namespace Anna.Gui.Messaging.Messages;

public class MessageBase
{
    public readonly string MessageKey;
    
    public MessageBase(string messageKey)
    {
        MessageKey = messageKey;
    }
}
using Anna.UseCase;
using Avalonia;

namespace Anna.Gui.ViewModels.Messaging;

public class InteractionMessage : AvaloniaObject
{
    public static readonly StyledProperty<string> MessageKeyProperty =
        AvaloniaProperty.Register<InteractionMessage, string>(nameof(MessageKey));
    
    internal IServiceProviderContainer? ServiceProviderContainer { get; set; }

    public InteractionMessage(string messageKey)
    {
        MessageKey = messageKey;
    }

    public string MessageKey
    {
        get => GetValue(MessageKeyProperty);
        init => SetValue(MessageKeyProperty, value);
    }
}
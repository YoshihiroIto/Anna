using Anna.Gui.Messaging.Behaviors.Actions;
using Anna.Gui.Messaging.Messages;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;
using System;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging.Behaviors;

public sealed class InteractionMessageTrigger : Trigger<Control>
{
    public static readonly StyledProperty<InteractionMessenger> MessengerProperty =
        AvaloniaProperty.Register<InteractionMessageTrigger, InteractionMessenger>(
            nameof(Messenger),
            defaultBindingMode: BindingMode.OneTime);

    public InteractionMessenger Messenger
    {
        get => GetValue(MessengerProperty);
        set => SetValue(MessengerProperty, value);
    }

    public string MessageKey { get; set; } = "";

    static InteractionMessageTrigger()
    {
        MessengerProperty.Changed.Subscribe(MessengerChanged);
    }

    private static void MessengerChanged(AvaloniaPropertyChangedEventArgs<InteractionMessenger> e)
    {
        if (e.Sender is not InteractionMessageTrigger self)
            return;

        var oldValue = e.OldValue.GetValueOrDefault();
        var newValue = e.NewValue.GetValueOrDefault();

        if (oldValue is not null)
            self.Messenger.Raised -= self.MessengerOnRaised;

        if (newValue is not null)
            self.Messenger.Raised += self.MessengerOnRaised;
    }
    
    private async ValueTask MessengerOnRaised(object? sender, InteractionMessage message)
    {
        if (message.MessageKey != MessageKey)
            return;

        foreach (var avaloniaObject in Actions)
        {
            if (avaloniaObject is not IAsyncAction action)
                throw new NotSupportedException();

            await action.ExecuteAsync(this, message, Messenger);
        }
    }
}
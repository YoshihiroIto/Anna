using Anna.Gui.ViewModels.Messaging;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;
using System;

namespace Anna.Gui.Views.Behaviors.Messaging;

public class InteractionMessageTrigger : Trigger<Control>
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
    private async void MessengerOnRaised(object? sender, InteractionMessageRaisedEventArgs e)
    {
        if (string.CompareOrdinal(e.Message.MessageKey, MessageKey) != 0)
            return;

        e.Message.ServiceProviderContainer = Messenger.ServiceProviderContainer;

        foreach (var avaloniaObject in Actions)
        {
            if (avaloniaObject is not IAsyncAction action)
                throw new NotSupportedException();

            await action.ExecuteAsync(this, e.Message);
        }
    }
}
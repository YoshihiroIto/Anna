using Anna.UseCase;
using System;

namespace Anna.Gui.ViewModels.Messaging;

public class InteractionMessenger : IHasServiceProviderContainer
{
    public IServiceProviderContainer ServiceProviderContainer { get; }
    
    public InteractionMessenger(IServiceProviderContainer dic)
    {
        ServiceProviderContainer = dic;
    }
    
    public event EventHandler<InteractionMessageRaisedEventArgs>? Raised;

    public void Raise(InteractionMessage message)
    {
        Raised?.Invoke(this, new InteractionMessageRaisedEventArgs(message));
    }
}

public class InteractionMessageRaisedEventArgs : EventArgs
{
    public InteractionMessageRaisedEventArgs(InteractionMessage message)
    {
        Message = message;
    }

    public readonly InteractionMessage Message;
}
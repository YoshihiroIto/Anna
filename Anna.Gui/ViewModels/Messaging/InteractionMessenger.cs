using Anna.UseCase;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Anna.Gui.ViewModels.Messaging;

public class InteractionMessenger : IHasServiceProviderContainer
{
    public IServiceProviderContainer ServiceProviderContainer { get; }

    public InteractionMessenger(IServiceProviderContainer dic)
    {
        ServiceProviderContainer = dic;
    }

    public event InteractionMessageRaisedEventHandler? Raised;

    public async ValueTask<T> RaiseAsync<T>(T message)
        where T : InteractionMessage
    {
        if (Raised is null)
            return message;

        foreach (var d in Raised.GetInvocationList())
        {
            if (d is not InteractionMessageRaisedEventHandler eventHandler)
                throw new InvalidOperationException();

            await eventHandler.Invoke(this, message);
        }

        return message;
    }

    public delegate ValueTask InteractionMessageRaisedEventHandler(object? sender, InteractionMessage message);
}

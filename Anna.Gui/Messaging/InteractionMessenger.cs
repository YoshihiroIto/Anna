using Anna.UseCase;
using System;
using System.Threading.Tasks;
using IServiceProvider=Anna.UseCase.IServiceProvider;

namespace Anna.Gui.Messaging;

public class InteractionMessenger : IHasServiceProviderContainer
{
    public IServiceProvider Dic { get; }

    public InteractionMessenger(IServiceProvider dic)
    {
        Dic = dic;
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

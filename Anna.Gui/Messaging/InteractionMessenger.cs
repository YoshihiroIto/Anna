using Anna.Gui.Messaging.Messages;
using Anna.Service;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Messaging;

public sealed class InteractionMessenger : IHasServiceProviderContainer
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

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            foreach (var d in Raised.GetInvocationList())
            {
                if (d is not InteractionMessageRaisedEventHandler eventHandler)
                    throw new InvalidOperationException();

                await eventHandler.Invoke(this, message);
            }
        });

        return message;
    }

    public delegate ValueTask InteractionMessageRaisedEventHandler(object? sender, InteractionMessage message);
}
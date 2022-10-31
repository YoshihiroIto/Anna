using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging;

public sealed class Messenger
{
    public event InteractionMessageRaisedEventHandler? Raised;

    public async ValueTask<T> RaiseAsync<T>(T message)
        where T : MessageBase
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

    public T Raise<T>(T message)
        where T : MessageBase
    {
        if (Raised is null)
            return message;

        using var m = new ManualResetEventSlim();

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            foreach (var d in Raised.GetInvocationList())
            {
                if (d is not InteractionMessageRaisedEventHandler eventHandler)
                    throw new InvalidOperationException();

                await eventHandler.Invoke(this, message);
            }

            // ReSharper disable once AccessToDisposedClosure
            m.Set();
        });

        m.Wait();

        return message;
    }

    public delegate ValueTask InteractionMessageRaisedEventHandler(object? sender, MessageBase message);
}
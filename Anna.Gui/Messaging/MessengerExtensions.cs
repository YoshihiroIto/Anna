using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using Anna.Service;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging;

public static class MessengerExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<TViewModel> RaiseTransitionAsync<TViewModel, TArg>(
        this Messenger messenger,
        TViewModel t,
        TArg arg,
        string messageKey)
        where TViewModel : WindowBaseViewModel, IHasArg<TArg>
    {
        var viewModel = messenger.Dic.GetInstance(t, arg);

        await messenger.RaiseAsync(new TransitionMessage(viewModel, messageKey));

        return viewModel;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TViewModel RaiseTransition<TViewModel, TArg>(
        this Messenger messenger,
        TViewModel t,
        TArg arg,
        string messageKey)
        where TViewModel : WindowBaseViewModel, IHasArg<TArg>
    {
        var viewModel = messenger.Dic.GetInstance(t, arg);

        messenger.Raise(new TransitionMessage(viewModel, messageKey));

        return viewModel;
    }
}
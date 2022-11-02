using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Windows.Base;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging;

public static class MessengerExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<T> RaiseTransitionAsync<T>(this Messenger messenger, T viewModel, string messageKey)
        where T : WindowBaseViewModel
    {
        await messenger.RaiseAsync(new TransitionMessage(viewModel, messageKey));

        return viewModel;
    }
}
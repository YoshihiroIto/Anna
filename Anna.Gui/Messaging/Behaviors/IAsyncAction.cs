using Anna.Gui.Messaging.Messages;
using Anna.Service;
using Avalonia.Xaml.Interactivity;
using System.Threading.Tasks;

namespace Anna.Gui.Messaging.Behaviors;

public interface IAsyncAction
{
    public ValueTask ExecuteAsync(
        Trigger sender,
        InteractionMessage message,
        IHasServiceProviderContainer hasServiceProviderContainer);
}
using Anna.Gui.ViewModels.Messaging;
using Avalonia.Xaml.Interactivity;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Behaviors.Messaging;

public interface IAsyncAction
{
    public ValueTask ExecuteAsync(Trigger sender, InteractionMessage message);
}
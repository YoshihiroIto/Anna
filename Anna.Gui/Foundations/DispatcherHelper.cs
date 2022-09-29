using Avalonia.Threading;
using System.Threading.Tasks;

namespace Anna.Gui.Foundations;

public static class DispatcherHelper
{
    public static Task DoEventsAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() => {}, DispatcherPriority.Normal);
    }
}
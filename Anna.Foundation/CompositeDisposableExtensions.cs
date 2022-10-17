using System.Reactive.Disposables;

namespace Anna.Foundation;

public static class CompositeDisposableExtensions
{
    public static void AddTo(this CompositeDisposable c, Action action)
    {
        c.Add(Disposable.Create(action));
    }
}
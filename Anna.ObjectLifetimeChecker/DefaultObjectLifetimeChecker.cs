#if DEBUG
using Anna.UseCase;
using System.Collections.Concurrent;
using System.Text;

namespace Anna.ObjectLifetimeChecker;

public sealed class DefaultObjectLifetimeChecker : IObjectLifetimeCheckerUseCase
{
    private ConcurrentDictionary<IDisposable, byte> Disposables =>
        LazyInitializer.EnsureInitialized(ref _disposables, () => new ConcurrentDictionary<IDisposable, byte>()) ??
        throw new NullReferenceException();

    private ConcurrentDictionary<IDisposable, byte>? _disposables;
    private Action<string>? _showError;
    private int _nestCount;
    
    public void Start(Action<string> showError)
    {
        var old = Interlocked.Exchange(ref _nestCount, 1);
        if (old != 0)
            throw new NestingException();

        _showError = showError;
    }

    public void End()
    {
        if (Disposables.Count > 0)
        {
            var sb = new StringBuilder();
            foreach (var d in Disposables.Keys)
            {
                sb.Append("    ");
                sb.AppendLine(d.GetType().ToString());
            }

            _showError?.Invoke($"Found undisposed object(s).\n{sb}");
        }

        Disposables.Clear();

        var old = Interlocked.Exchange(ref _nestCount, 0);
        if (old != 1)
            throw new NestingException();
    }

    public void Add(IDisposable disposable)
    {
        if (Disposables.ContainsKey(disposable))
            _showError?.Invoke($"Found multiple addition.    -- {disposable.GetType()}");

        Disposables[disposable] = 0;
    }

    public void Remove(IDisposable disposable)
    {
        if (Disposables.ContainsKey(disposable) == false)
            _showError?.Invoke($"Found multiple removing.    -- {disposable.GetType()}");

        Disposables.TryRemove(disposable, out _);
    }
}

public class NestingException : Exception
{
}

#endif
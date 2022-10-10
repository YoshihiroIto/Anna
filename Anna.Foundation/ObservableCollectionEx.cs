using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Anna.Foundation;

public sealed class ObservableCollectionEx<T> : ObservableCollection<T>
{
    public void BeginChange()
    {
        Interlocked.Increment(ref _changingDepth);
    }

    public void EndChange()
    {
        var c = Interlocked.Decrement(ref _changingDepth);

        Debug.Assert(c >= 0);

        if (c == 0)
            OnCollectionChanged(ResetEventArgs);
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            Items.Add(item);

        if (IsInChanging == false)
            OnCollectionChanged(ResetEventArgs);
    }

    public new void Clear()
    {
        if (IsInChanging)
        {
            if (Items.Count > 0)
                Items.Clear();
        }
        else
            base.Clear();
    }

#if false
    public new bool Remove(T item)
    {
        if (IsInChanging)
        {
            if (Items.Remove(item))
                return true;
            else
                return false;
        }
        else
            return base.Remove(item);
    }

    public new void Insert(int index, T item)
    {
        if (IsInChanging)
            Items.Insert(index, item);
        else
            base.Insert(index, item);
    }
#endif

    public Span<T> AsSpan()
    {
        if (Items is not List<T> list)
            throw new NotSupportedException();

        return CollectionsMarshal.AsSpan(list);
    }

    private bool IsInChanging => _changingDepth > 0;
    private int _changingDepth;

    // ReSharper disable once StaticMemberInGenericType
    private static readonly NotifyCollectionChangedEventArgs ResetEventArgs =
        new(NotifyCollectionChangedAction.Reset);
}
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Anna.DomainModel.Foundation;

public class NotificationObject : INotifyPropertyChanged
{
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;

        RaisePropertyChanged(propertyName);

        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        // ReSharper disable once InconsistentlySynchronizedField
        var pc = (PropertyChangedEventArgs?)PropChanged[propertyName];

        if (pc == null)
        {
            // double-checked;
            lock (PropChangedLockObj)
            {
                pc = (PropertyChangedEventArgs?)PropChanged[propertyName];

                if (pc == null)
                {
                    pc = new PropertyChangedEventArgs(propertyName);
                    PropChanged[propertyName] = pc;
                }
            }
        }

        PropertyChanged?.Invoke(this, pc);
    }

    // use Hashtable to get free lockless reading
    private static readonly Hashtable PropChanged = new();
    private static readonly object PropChangedLockObj = new();
}
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Anna.Foundation;

public class NotificationObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // use Hashtable to get free lockless reading
    private static readonly Hashtable PropChanged = new();
    private static FastSpinLock _lockObj;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;

        if (PropertyChanged is not null)
            RaisePropertyChanged(propertyName);

        return true;
    }

    protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (PropertyChanged is null)
            return;

        // ReSharper disable once InconsistentlySynchronizedField
        var pc = (PropertyChangedEventArgs?)PropChanged[propertyName];

        if (pc is null)
        {
            try
            {
                _lockObj.Enter();

                pc = (PropertyChangedEventArgs?)PropChanged[propertyName];

                if (pc is null)
                {
                    pc = new PropertyChangedEventArgs(propertyName);
                    PropChanged[propertyName] = pc;
                }
            }
            finally
            {
                _lockObj.Exit();
            }
        }

        PropertyChanged.Invoke(this, pc);
    }
}
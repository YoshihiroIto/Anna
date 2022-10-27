using Anna.Service.Interfaces;
using System.ComponentModel;

namespace Anna.Service.Workers;

public interface IBackgroundWorker : INotifyPropertyChanged, IIsTransient
{
    event EventHandler<ExceptionThrownEventArgs>? ExceptionThrown;
    
    bool IsInProcessing { get; }
    double Progress { get; }

    ValueTask PushOperatorAsync(IBackgroundOperator @operator);
}

public sealed class ExceptionThrownEventArgs : EventArgs
{
    public readonly Exception Exception;
    
    public ExceptionThrownEventArgs(Exception exception)
    {
        Exception = exception;
    }
}
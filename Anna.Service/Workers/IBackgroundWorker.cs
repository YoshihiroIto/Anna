using Anna.Service.Interfaces;
using System.ComponentModel;

namespace Anna.Service.Workers;

public interface IBackgroundWorker : INotifyPropertyChanged, IIsTransient
{
    bool IsInProcessing { get; }
    double Progress { get; }

    ValueTask PushOperatorAsync(IBackgroundOperator @operator);
}
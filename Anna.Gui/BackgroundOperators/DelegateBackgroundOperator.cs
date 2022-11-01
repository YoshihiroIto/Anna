using Anna.Foundation;
using Anna.Service.Interfaces;
using System;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.BackgroundOperators;

public class DelegateBackgroundOperator : HasArgDisposableNotificationObject<DelegateBackgroundOperator, Action>
    , IBackgroundOperator
{
    #region Progress

    private double _Progress;

    public double Progress
    {
        get => _Progress;
        set => SetProperty(ref _Progress, value);
    }

    #endregion

    public DelegateBackgroundOperator(IServiceProvider dic) : base(dic)
    {
        Progress = 0;
    }

    public ValueTask ExecuteAsync()
    {
        Arg.Invoke();
        return ValueTask.CompletedTask;
    }
}
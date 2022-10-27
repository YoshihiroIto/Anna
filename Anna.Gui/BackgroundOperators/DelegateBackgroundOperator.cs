using Anna.Foundation;
using Anna.Service.Interfaces;
using System;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.BackgroundOperators;

public class DelegateBackgroundOperator
    : HasArgDisposableNotificationObject<Action>
        , IBackgroundOperator
{
    #region Progress

    private readonly double _Progress;

    public double Progress
    {
        get => _Progress;
        init => SetProperty(ref _Progress, value);
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
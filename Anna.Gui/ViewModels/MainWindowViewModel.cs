using Anna.DomainModel.Interface;
using Anna.Foundations;
using Anna.Interactor.Foundations;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;

namespace Anna.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReadOnlyReactiveProperty<string?> Caption { get; }
    public ReactiveCommand CountUp { get; }
    private ReactiveProperty<int> Count { get; }

    public MainWindowViewModel(IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        Count = new ReactiveProperty<int>().AddTo(Trash);

        CountUp = new ReactiveCommand()
            .WithSubscribe(() => ++Count.Value)
            .AddTo(Trash);

        Caption = Count
            .Select(x => $"Anna:{x}")
            .ToReadOnlyReactiveProperty()
            .AddTo(Trash);
    }
}

public class DesignMainWindowViewModel : MainWindowViewModel
{
    public DesignMainWindowViewModel()
        : base(new NopObjectLifetimeChecker())
    {
    }
}

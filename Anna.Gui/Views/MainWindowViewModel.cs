using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;

namespace Anna.Gui.Views;

public class MainWindowViewModel : ViewModelBase, ILocalizableViewModel
{
    private readonly ResourcesHolder _resourcesHolder;
    public Resources R => _resourcesHolder.Instance;

    public ReadOnlyReactiveProperty<string?> Caption { get; }
    public ReactiveCommand CountUp { get; }
    private ReactiveProperty<int> Count { get; }

    public MainWindowViewModel(
        ResourcesHolder resourcesHolder,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _resourcesHolder = resourcesHolder;

        Observable
            .FromEventPattern(
                h => _resourcesHolder.CultureChanged += h,
                h => _resourcesHolder.CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

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
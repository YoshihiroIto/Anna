using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.Views.Panels;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using SimpleInjector;
using System;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Anna.Gui.Views.Windows;

public class DirectoryWindowViewModel : ViewModelBase, ILocalizableViewModel
{
    #region DirectoryPanelViewModel

    private DirectoryPanelViewModel? _directoryPanelViewModel;

    public DirectoryPanelViewModel? DirectoryPanelViewModel
    {
        get => _directoryPanelViewModel;
        private set
        {
            var old = _directoryPanelViewModel;
            if (SetProperty(ref _directoryPanelViewModel, value))
                old?.Dispose();
        }
    }

    #endregion

    public Resources R => _resourcesHolder.Instance;

    public ICommand ToEnglishCommand { get; }
    public ICommand ToJapaneseCommand { get; }

    // todo:impl Messenger
    public event EventHandler? Close;

    public DirectoryWindowViewModel(
        Container dic,
        ResourcesHolder resourcesHolder,
        AppConfig appConfig,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _dic = dic;
        _resourcesHolder = resourcesHolder;

        Observable
            .FromEventPattern(
                h => _resourcesHolder.CultureChanged += h,
                h => _resourcesHolder.CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

        ToEnglishCommand = new DelegateCommand(() => appConfig.Data.Culture = Cultures.En);
        ToJapaneseCommand = new DelegateCommand(() => appConfig.Data.Culture = Cultures.Ja);
    }

    public DirectoryWindowViewModel Setup(Directory model)
    {
        _model = model;

        DirectoryPanelViewModel = _dic.GetInstance<DirectoryPanelViewModel>()
            .Setup(model);

        _dic.GetInstance<App>().Directories.CollectionChangedAsObservable()
            .Subscribe(_ =>
            {
                if (_dic.GetInstance<App>().Directories.IndexOf(_model) == -1)
                    Close?.Invoke(this, EventArgs.Empty);
            }).AddTo(Trash);

        return this;
    }

    public override void Dispose()
    {
        if (_isDispose)
            return;

        _isDispose = true;

        _dic.GetInstance<App>().CloseDirectory(_model ?? throw new NullReferenceException());

        DirectoryPanelViewModel = null;

        base.Dispose();
    }

    private bool _isDispose;
    private readonly Container _dic;
    private readonly ResourcesHolder _resourcesHolder;
    private Directory? _model;
}
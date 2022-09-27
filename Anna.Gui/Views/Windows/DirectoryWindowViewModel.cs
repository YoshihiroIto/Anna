using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.Views.Panels;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Anna.Gui.Views.Windows;

public class DirectoryWindowViewModel : HasModelRefViewModelBase<Directory>, ILocalizableViewModel
{
    #region DirectoryPanelViewModel

    private readonly DirectoryPanelViewModel? _directoryPanelViewModel;

    public DirectoryPanelViewModel DirectoryPanelViewModel
    {
        get => _directoryPanelViewModel ?? throw new NullReferenceException();
        private init => SetProperty(ref _directoryPanelViewModel, value);
    }

    #endregion

    #region InfoPanelViewModel

    private readonly InfoPanelViewModel? _InfoPanelViewModel;

    public InfoPanelViewModel InfoPanelViewModel
    {
        get => _InfoPanelViewModel ?? throw new NullReferenceException();
        private init => SetProperty(ref _InfoPanelViewModel, value);
    }

    #endregion

    public Resources R => _resourcesHolder.Instance;

    public ICommand ToEnglishCommand { get; }
    public ICommand ToJapaneseCommand { get; }

    // todo:impl Messenger
    public event EventHandler? CloseRequested;

    public DirectoryWindowViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        AppConfig appConfig,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
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
        
        InfoPanelViewModel = _dic.GetInstance<InfoPanelViewModel, Directory>(Model)
            .AddTo(Trash);

        DirectoryPanelViewModel = _dic.GetInstance<DirectoryPanelViewModel, Directory>(Model)
            .AddTo(Trash);

        _dic.GetInstance<App>().Directories.CollectionChangedAsObservable()
            .Subscribe(_ =>
            {
                if (_dic.GetInstance<App>().Directories.IndexOf(Model) == -1)
                    CloseRequested?.Invoke(this, EventArgs.Empty);
            }).AddTo(Trash);
    }

    public override void Dispose()
    {
        if (_isDispose)
            return;

        _isDispose = true;

        _dic.GetInstance<App>().CloseDirectory(Model);

        base.Dispose();
    }

    private bool _isDispose;
    private readonly IServiceProviderContainer _dic;
    private readonly ResourcesHolder _resourcesHolder;
}
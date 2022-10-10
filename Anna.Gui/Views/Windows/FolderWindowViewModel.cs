using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Gui.Views.Panels;
using Anna.Strings;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Anna.Gui.Views.Windows;

public class FolderWindowViewModel : HasModelRefViewModelBase<Folder>, ILocalizableViewModel
{
    public Resources R => _resourcesHolder.Instance;

    public FolderPanelViewModel FolderPanelViewModel { get; }
    public InfoPanelViewModel InfoPanelViewModel { get; }

    public ICommand ToEnglishCommand { get; }
    public ICommand ToJapaneseCommand { get; }

    private readonly IServiceProviderContainer _dic;
    private readonly ResourcesHolder _resourcesHolder;
    private bool _isDispose;

    public FolderWindowViewModel(
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

        InfoPanelViewModel = _dic.GetInstance<InfoPanelViewModel, Folder>(Model)
            .AddTo(Trash);

        FolderPanelViewModel = _dic.GetInstance<FolderPanelViewModel, Folder>(Model)
            .AddTo(Trash);

        _dic.GetInstance<App>().Folders.CollectionChangedAsObservable()
            .Subscribe(_ =>
            {
                if (_dic.GetInstance<App>().Folders.IndexOf(Model) == -1)
 #pragma warning disable CS4014
                    Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, DialogViewModel.MessageKeyClose));
 #pragma warning restore CS4014
            }).AddTo(Trash);
    }

    public override void Dispose()
    {
        if (_isDispose)
            return;

        _isDispose = true;

        _dic.GetInstance<App>().CloseFolder(Model);

        base.Dispose();
    }
}
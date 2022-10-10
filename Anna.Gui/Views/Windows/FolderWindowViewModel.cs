using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows.Input;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows;

public class FolderWindowViewModel : HasModelWindowViewModelBase<Folder>
{
    public FolderPanelViewModel FolderPanelViewModel { get; }
    public InfoPanelViewModel InfoPanelViewModel { get; }

    public ICommand ToEnglishCommand { get; }
    public ICommand ToJapaneseCommand { get; }

    private bool _isDispose;

    public FolderWindowViewModel(IServiceProvider dic)
        : base(dic)
    {
        Observable
            .FromEventPattern(
                h => Dic.GetInstance<ResourcesHolder>().CultureChanged += h,
                h => Dic.GetInstance<ResourcesHolder>().CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

        ToEnglishCommand = new DelegateCommand(() => Dic.GetInstance<AppConfig>().Data.Culture = Cultures.En);
        ToJapaneseCommand = new DelegateCommand(() => Dic.GetInstance<AppConfig>().Data.Culture = Cultures.Ja);

        InfoPanelViewModel = dic.GetInstance<InfoPanelViewModel, Folder>(Model)
            .AddTo(Trash);

        FolderPanelViewModel = dic.GetInstance<FolderPanelViewModel, Folder>(Model)
            .AddTo(Trash);
        
        dic.GetInstance<App>().Folders.CollectionChangedAsObservable()
            .Subscribe(_ =>
            {
                if (dic.GetInstance<App>().Folders.IndexOf(Model) == -1)
 #pragma warning disable CS4014
                    Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, WindowViewModelBase.MessageKeyClose));
 #pragma warning restore CS4014
            }).AddTo(Trash);
    }

    public override void Dispose()
    {
        if (_isDispose)
            return;

        _isDispose = true;

        Dic.GetInstance<App>().RemoveFolder(Model);

        base.Dispose();
    }
}
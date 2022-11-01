using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows.Input;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows;

public sealed class FolderWindowViewModel : HasModelWindowBaseViewModel<FolderWindowViewModel,
    (Folder Folder, int Dummy)>
{
    public FolderPanelViewModel FolderPanelViewModel { get; }
    public InfoPanelViewModel InfoPanelViewModel { get; }

    public ICommand ToEnglishCommand { get; }
    public ICommand ToJapaneseCommand { get; }

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

        InfoPanelViewModel = dic.GetInstance(InfoPanelViewModel.T, Model.Folder)
            .AddTo(Trash);

        FolderPanelViewModel = dic.GetInstance(FolderPanelViewModel.T, Model.Folder)
            .AddTo(Trash);

        dic.GetInstance<App>().Folders.CollectionChangedAsObservable()
            .Subscribe(_ =>
            {
                if (dic.GetInstance<App>().Folders.IndexOf(Model.Folder) == -1)
                    Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKey.Close)).Forget();
            }).AddTo(Trash);

        Trash.Add(() =>
        {
            Dic.GetInstance<App>().RemoveFolder(Model.Folder);
        });
    }
}
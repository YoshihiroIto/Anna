using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging.Messages;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Anna.Localization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows.Input;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows;

public sealed class FolderWindowViewModel : HasModelWindowBaseViewModel<FolderWindowViewModel,
    (Folder Folder, int Dummy)>
{
    public int Id => Model.Folder.Id;
    
    public ReadOnlyReactivePropertySlim<string> Title { get; }
    
    public FolderPanelViewModel FolderPanelViewModel { get; }
    public InfoPanelViewModel InfoPanelViewModel { get; }

    public ICommand ToEnglishCommand { get; }
    public ICommand ToJapaneseCommand { get; }

    public FolderWindowViewModel(IServiceProvider dic)
        : base(dic)
    {
        ToEnglishCommand = new DelegateCommand(() => Dic.GetInstance<AppConfig>().Data.Culture = Cultures.En);
        ToJapaneseCommand = new DelegateCommand(() => Dic.GetInstance<AppConfig>().Data.Culture = Cultures.Ja);

        InfoPanelViewModel = dic.GetInstance(InfoPanelViewModel.T, Model.Folder)
            .AddTo(Trash);

        FolderPanelViewModel = dic.GetInstance(FolderPanelViewModel.T, Model.Folder)
            .AddTo(Trash);

        Title = FolderPanelViewModel.CurrentFolderPath
            .Select(x => $"{x} -- {Resources.AppName}")
            .ToReadOnlyReactivePropertySlim("")
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
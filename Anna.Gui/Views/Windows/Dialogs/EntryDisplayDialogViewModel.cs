using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System;
using System.IO;

namespace Anna.Gui.Views.Windows.Dialogs;

public class EntryDisplayDialogViewModel
    : HasModelWindowViewModelBase<Entry>, IHasArg<(string CurrentFolderPath, JumpFolderConfigData Config)>
{
    public string Title => Model.NameWithExtension + " - " + Path.GetDirectoryName(Model.Path);

    public ViewModelBase? ContentViewModel { get; }

    public EntryDisplayDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        AppConfig appConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, appConfig, logger, objectLifetimeChecker)
    {
        switch (Model.Format)
        {
            case FileEntryFormat.Image:
                ContentViewModel = dic.GetInstance<ImageViewerViewModel, Entry>(Model).AddTo(Trash);
                break;
            
            case FileEntryFormat.Text:
                ContentViewModel = dic.GetInstance<TextViewerViewModel, Entry>(Model).AddTo(Trash);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
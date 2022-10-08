using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;
using System.IO;

namespace Anna.Gui.Views.Dialogs;

public class EntryDisplayDialogViewModel
    : HasModelDialogViewModel<Entry>, IHasArg<(string CurrentFolderPath, JumpFolderConfigData Config)>
{
    public string Title => Model.NameWithExtension + " - " + Path.GetDirectoryName(Model.Path);

    public EntryDisplayDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, logger, objectLifetimeChecker)
    {
    }
}
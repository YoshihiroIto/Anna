using Anna.Constants;
using Anna.DomainModel.FileSystem;
using Anna.Gui.Views.Windows.Base;
using Anna.Service;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class SelectFileCopyActionDialogViewModel
    : HasModelWindowBaseViewModel<(string SrcFilepath,  string DestFilepath)>
{
    public FileSystemCopier.CopyStrategyWhenExistsResult Result { get; private set; } =
        new(ExistsCopyFileActions.Skip, "", false);

    public SelectFileCopyActionDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
    }
}
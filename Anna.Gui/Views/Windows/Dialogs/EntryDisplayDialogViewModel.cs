using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Anna.Service;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class EntryDisplayDialogViewModel
    : HasModelWindowBaseViewModel<Entry>, IHasArg<(string CurrentFolderPath, JumpFolderConfigData Config)>
{
    public string Title => Model.NameWithExtension + " - " + Path.GetDirectoryName(Model.Path);

    public ViewModelBase? ContentViewModel { get; }

    public EntryDisplayDialogViewModel(IServiceProvider dic)
        : base(dic)
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
using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Views.Panels;
using Anna.Gui.Views.Windows.Base;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class EntryDisplayDialogViewModel : HasModelWindowBaseViewModel<EntryDisplayDialogViewModel,
    (Entry Entry, int Dummy)>
{
    public string Title => Model.Entry.NameWithExtension + " - " + Path.GetDirectoryName(Model.Entry.Path);

    public ViewModelBase? ContentViewModel { get; }

    public EntryDisplayDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
        switch (Model.Entry.Format)
        {
            case FileEntryFormat.Image:
                ContentViewModel = dic.GetInstance(ImageViewerViewModel.T, Model.Entry).AddTo(Trash);
                break;

            case FileEntryFormat.Text:
                ContentViewModel = dic.GetInstance(TextViewerViewModel.T, Model.Entry).AddTo(Trash);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
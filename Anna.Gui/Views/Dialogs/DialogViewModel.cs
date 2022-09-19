using Anna.DomainModel.Interfaces;
using Anna.Gui.Foundations;

namespace Anna.Gui.Views.Dialogs;

public class DialogViewModel : ViewModelBase
{
    public DialogResultTypes DialogResult { get; set; } = DialogResultTypes.Cancel;

    protected DialogViewModel(IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
    }
}

public enum DialogResultTypes
{
    Ok,
    Cancel
}
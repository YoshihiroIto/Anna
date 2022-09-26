using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Strings;
using Anna.UseCase;

namespace Anna.Gui.Views.Panels;

public class InfoPanelViewModel : ViewModelBase, ILocalizableViewModel
{
    public Resources R => _resourcesHolder.Instance;

    protected InfoPanelViewModel(
        ResourcesHolder resourcesHolder,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker) : base(objectLifetimeChecker)
    {
        _resourcesHolder = resourcesHolder;
    }

    private readonly ResourcesHolder _resourcesHolder;
}
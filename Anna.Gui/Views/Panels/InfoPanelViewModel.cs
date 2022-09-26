using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Strings;
using Anna.UseCase;

namespace Anna.Gui.Views.Panels;

public class InfoPanelViewModel : ViewModelBase, ILocalizableViewModel
{
    public Directory Model { get; private set; } = null!;
    public Resources R => _resourcesHolder.Instance;

    public InfoPanelViewModel(
        ResourcesHolder resourcesHolder,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker) : base(objectLifetimeChecker)
    {
        _resourcesHolder = resourcesHolder;
    }

    public InfoPanelViewModel Setup(Directory model)
    {
        Model = model;

        return this;
    }

    private readonly ResourcesHolder _resourcesHolder;
}
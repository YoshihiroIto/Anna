using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.ShortcutKey;
using Anna.UseCase;

namespace Anna.Gui.Views.Panels;

public class TextViewerViewModel : HasModelRefViewModelBase<Entry>
{
    public readonly TextViewerShortcutKey ShortcutKey;

    public TextViewerViewModel(
        IServiceProviderContainer dic,
        TextViewerShortcutKey textViewerShortcutKey,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        ShortcutKey = textViewerShortcutKey;
    }
}

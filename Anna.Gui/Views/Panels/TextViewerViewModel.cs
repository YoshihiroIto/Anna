using Anna.DomainModel;
using Anna.Gui.Foundations;
using Anna.Gui.ShortcutKey;
using Anna.UseCase;
using Reactive.Bindings.Extensions;

namespace Anna.Gui.Views.Panels;

public class TextViewerViewModel : HasModelRefViewModelBase<Entry>
{
    public readonly TextViewerShortcutKey ShortcutKey;

    public TextViewerViewModel(
        IServiceProviderContainer dic,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        ShortcutKey = dic.GetInstance<TextViewerShortcutKey>().AddTo(Trash);
    }
}

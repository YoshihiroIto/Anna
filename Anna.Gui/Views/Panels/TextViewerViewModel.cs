using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.ShortcutKey;
using Anna.Service;
using Anna.Strings;
using Reactive.Bindings.Extensions;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Panels;

public sealed class TextViewerViewModel : HasModelRefViewModelBase<Entry>, ILocalizableViewModel
{
    public Resources R => Dic.GetInstance<ResourcesHolder>().Instance;

    public readonly TextViewerShortcutKey ShortcutKey;

    public async ValueTask<string> ReadText()
    {
        await using var stream = Model.OpenRead();

        (string result, bool isText) = await StringHelper.BuildString(
            stream,
            Dic.GetInstance<AppConfig>().Data.TextViewerMaxBufferSize,
            "\n\n" + Resources.Message_OmittedDueToLargeSize);

        return isText ? result : Resources.Message_BinaryFileCannotBePreviewed;
    }

    public TextViewerViewModel(IServiceProvider dic)
        : base(dic)
    {
        ShortcutKey = dic.GetInstance<TextViewerShortcutKey>().AddTo(Trash);
    }
}
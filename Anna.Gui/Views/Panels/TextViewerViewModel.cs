using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.ShortcutKey;
using Anna.Strings;
using Anna.UseCase;
using Avalonia.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Threading.Tasks;

namespace Anna.Gui.Views.Panels;

public class TextViewerViewModel : HasModelRefViewModelBase<Entry>, ILocalizableViewModel
{
    public Resources R => _resourcesHolder.Instance;

    public ReadOnlyReactiveProperty<FontFamily> ViewerFontFamily { get; }
    public ReadOnlyReactiveProperty<double> ViewerFontSize { get; }

    public readonly TextViewerShortcutKey ShortcutKey;

    private readonly ResourcesHolder _resourcesHolder;
    private readonly AppConfig _appConfig;

    public async ValueTask<string> ReadText()
    {
        await using var stream = Model.OpenRead();

        (string result, bool isText) = await StringHelper.BuildString(
            stream,
            _appConfig.Data.TextViewerMaxBufferSize,
            "\n\n" + Resources.Message_OmittedDueToLargeSize);

        return isText ? result : Resources.Message_BinaryFileCannotBePreviewed;
    }

    public TextViewerViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        AppConfig appConfig,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        _resourcesHolder = resourcesHolder;
        _appConfig = appConfig;

 #pragma warning disable CS8619
        ViewerFontFamily = _appConfig.Data
            .ObserveProperty(x => x.ViewerFontFamily)
            .ToReadOnlyReactiveProperty()
            .AddTo(Trash);
 #pragma warning restore CS8619

        ViewerFontSize = _appConfig.Data
            .ObserveProperty(x => x.ViewerFontSize)
            .ToReadOnlyReactiveProperty()
            .AddTo(Trash);

        ShortcutKey = dic.GetInstance<TextViewerShortcutKey>().AddTo(Trash);
    }
}
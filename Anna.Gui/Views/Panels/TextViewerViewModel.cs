using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.Hotkey;
using Anna.Localization;
using Anna.Service;
using Avalonia.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Threading.Tasks;
using Anna.Service.Services;

namespace Anna.Gui.Views.Panels;

public sealed class TextViewerViewModel : HasModelViewModelBase<TextViewerViewModel, Entry>, ILocalizableViewModel
{
    public Resources R => Dic.GetInstance<ResourcesHolder>().Instance;

    public readonly TextViewerHotkey Hotkey;

    public bool ShowLineNumbers { get; init; }
    
    public ReadOnlyReactivePropertySlim<FontFamily> ViewerFontFamily { get; }
    public ReadOnlyReactivePropertySlim<double> ViewerFontSize { get; }

    public async ValueTask<string> ReadText()
    {
        return Constants.Constants.SupportedArchiveFormats.Contains(Model.Extension)
            ? await ReadTextArchive()
            : await ReadTextDefault();
    }

    public async ValueTask<string> ReadTextArchive()
    {
        return await Task.Run(() => Dic.GetInstance<ICompressorService>().ReadMetaString(Model.Path));
    }

    public async ValueTask<string> ReadTextDefault()
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
        Hotkey = dic.GetInstance<TextViewerHotkey>().AddTo(Trash);
        ShowLineNumbers = Constants.Constants.SupportedArchiveFormats.Contains(Model.Extension) == false;

        ViewerFontFamily = Dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.ViewerFontFamily)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash)!;

        ViewerFontSize = Dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.ViewerFontSize)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);
    }
}
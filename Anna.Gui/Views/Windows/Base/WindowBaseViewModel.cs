using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.Messaging.Messages;
using Anna.Localization;
using Anna.Service.Services;
using Avalonia.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows.Input;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Base;

public class WindowBaseViewModel : ViewModelBase, ILocalizableViewModel
{
    public const string MessageKeyClose = nameof(MessageKeyClose);
    public const string MessageKeyConfirmation = nameof(MessageKeyConfirmation);
    public const string MessageKeyJumpFolder = nameof(MessageKeyJumpFolder);
    public const string MessageKeySelectFolder = nameof(MessageKeySelectFolder);
    public const string MessageKeyChangeEntryName = nameof(MessageKeyChangeEntryName);
    public const string MessageKeySelectFileCopyAction = nameof(MessageKeySelectFileCopyAction);

    public Resources R => Dic.GetInstance<ResourcesHolder>().Instance;

    public ReadOnlyReactivePropertySlim<FontFamily> ViewerFontFamily => _ViewerFontFamily ??= CreateViewerFontFamily();
    public ReadOnlyReactivePropertySlim<double> ViewerFontSize => _ViewerFontSize ??= CreateViewerFontSize();

    public DialogResultTypes DialogResult { get; set; } = DialogResultTypes.Cancel;

    private ReadOnlyReactivePropertySlim<FontFamily>? _ViewerFontFamily;
    private ReadOnlyReactivePropertySlim<double>? _ViewerFontSize;

    protected WindowBaseViewModel(IServiceProvider dic)
        : base(dic)
    {
        Observable
            .FromEventPattern(
                h => Dic.GetInstance<ResourcesHolder>().CultureChanged += h,
                h => Dic.GetInstance<ResourcesHolder>().CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

        Dic.GetInstance<ILoggerService>().Start(GetType().Name);

        Trash.Add(() =>
        {
            Dic.GetInstance<ILoggerService>().End(GetType().Name);
        });
    }

    protected ICommand CreateButtonCommand(DialogResultTypes result)
    {
        return new AsyncDelegateCommand(async () =>
        {
            DialogResult = result;

            await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
        });
    }

    private ReadOnlyReactivePropertySlim<FontFamily> CreateViewerFontFamily()
    {
        return Dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.ViewerFontFamily)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash)!;
    }

    private ReadOnlyReactivePropertySlim<double> CreateViewerFontSize()
    {
        return Dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.ViewerFontSize)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Trash);
    }
}
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.Messaging;
using Anna.Localization;
using Anna.Service;
using Avalonia.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Base;

public class WindowBaseViewModel : ViewModelBase, ILocalizableViewModel
{
    public const string MessageKeyClose = nameof(MessageKeyClose);
    public const string MessageKeyInformation = nameof(MessageKeyInformation);
    public const string MessageKeyYesNoConfirmation = nameof(MessageKeyYesNoConfirmation);
    public const string MessageKeyJumpFolder = nameof(MessageKeyJumpFolder);
    public const string MessageKeySelectFolder = nameof(MessageKeySelectFolder);

    public Resources R => Dic.GetInstance<ResourcesHolder>().Instance;

    public DelegateCommand OkCommand => _OkCommand ??= CreateButtonCommand(DialogResultTypes.Ok);
    public DelegateCommand CancelCommand => _CancelCommand ??= CreateButtonCommand(DialogResultTypes.Cancel);
    public DelegateCommand YesCommand => _YesCommand ??= CreateButtonCommand(DialogResultTypes.Yes);
    public DelegateCommand NoCommand => _NoCommand ??= CreateButtonCommand(DialogResultTypes.No);

    public ReadOnlyReactivePropertySlim<FontFamily> ViewerFontFamily => _ViewerFontFamily ??= CreateViewerFontFamily();
    public ReadOnlyReactivePropertySlim<double> ViewerFontSize => _ViewerFontSize ??= CreateViewerFontSize();

    public DialogResultTypes DialogResult { get; set; } = DialogResultTypes.Cancel;

    protected DelegateCommand? _OkCommand;
    protected DelegateCommand? _CancelCommand;
    protected DelegateCommand? _YesCommand;
    protected DelegateCommand? _NoCommand;
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
    }

    public override void Dispose()
    {
        Dic.GetInstance<ILoggerService>().End(GetType().Name);

        base.Dispose();

        GC.SuppressFinalize(this);
    }

    private DelegateCommand CreateButtonCommand(DialogResultTypes result)
    {
        return new DelegateCommand(() =>
        {
            DialogResult = result;

 #pragma warning disable CS4014
            Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, MessageKeyClose));
 #pragma warning restore CS4014
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

public enum ConfirmationTypes
{
    YesNo
}

public enum DialogResultTypes
{
    Ok,
    Cancel,
    Yes,
    No
}
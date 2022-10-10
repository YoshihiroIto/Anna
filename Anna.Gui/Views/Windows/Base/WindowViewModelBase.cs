using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.Messaging;
using Anna.Service;
using Anna.Strings;
using Avalonia.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Windows.Base;

public class WindowViewModelBase : ViewModelBase, ILocalizableViewModel
{
    public const string MessageKeyClose = nameof(MessageKeyClose);
    public const string MessageKeyInformation = nameof(MessageKeyInformation);
    public const string MessageKeyYesNoConfirmation = nameof(MessageKeyYesNoConfirmation);

    public Resources R => Dic.GetInstance<ResourcesHolder>().Instance;

    public DelegateCommand OkCommand => _okCommand ??= CreateButtonCommand(DialogResultTypes.Ok);
    public DelegateCommand CancelCommand => _cancelCommand ??= CreateButtonCommand(DialogResultTypes.Cancel);
    public DelegateCommand YesCommand => _yesCommand ??= CreateButtonCommand(DialogResultTypes.Yes);
    public DelegateCommand NoCommand => _noCommand ??= CreateButtonCommand(DialogResultTypes.No);

    public ReadOnlyReactiveProperty<FontFamily> ViewerFontFamily => _ViewerFontFamily ??= CreateViewerFontFamily();
    public ReadOnlyReactiveProperty<double> ViewerFontSize => _ViewerFontSize ??= CreateViewerFontSize();

    public DialogResultTypes DialogResult { get; set; } = DialogResultTypes.Cancel;

    private DelegateCommand? _okCommand;
    private DelegateCommand? _cancelCommand;
    private DelegateCommand? _yesCommand;
    private DelegateCommand? _noCommand;
    private ReadOnlyReactiveProperty<FontFamily>? _ViewerFontFamily;
    private ReadOnlyReactiveProperty<double>? _ViewerFontSize;

    protected WindowViewModelBase(IServiceProvider dic)
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

    private ReadOnlyReactiveProperty<FontFamily> CreateViewerFontFamily()
    {
        return Dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.ViewerFontFamily)
            .ToReadOnlyReactiveProperty()
            .AddTo(Trash)!;
    }

    private ReadOnlyReactiveProperty<double> CreateViewerFontSize()
    {
        return Dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.ViewerFontSize)
            .ToReadOnlyReactiveProperty()
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
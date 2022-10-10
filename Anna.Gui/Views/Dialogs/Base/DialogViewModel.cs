using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.Messaging;
using Anna.Strings;
using Anna.UseCase;
using Avalonia.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;

namespace Anna.Gui.Views.Dialogs.Base;

public class DialogViewModel : ViewModelBase, ILocalizableViewModel
{
    public const string MessageKeyClose = nameof(MessageKeyClose);
    public const string MessageKeyInformation = nameof(MessageKeyInformation);
    public const string MessageKeyYesNoConfirmation = nameof(MessageKeyYesNoConfirmation);

    public Resources R => _resourcesHolder.Instance;

    public DelegateCommand OkCommand => _okCommand ??= CreateButtonCommand(DialogResultTypes.Ok);
    public DelegateCommand CancelCommand => _cancelCommand ??= CreateButtonCommand(DialogResultTypes.Cancel);
    public DelegateCommand YesCommand => _yesCommand ??= CreateButtonCommand(DialogResultTypes.Yes);
    public DelegateCommand NoCommand => _noCommand ??= CreateButtonCommand(DialogResultTypes.No);

    public ReadOnlyReactiveProperty<FontFamily> ViewerFontFamily { get; }
    public ReadOnlyReactiveProperty<double> ViewerFontSize { get; }

    public DialogResultTypes DialogResult { get; set; } = DialogResultTypes.Cancel;

    private readonly ResourcesHolder _resourcesHolder;
    private readonly ILoggerUseCase _logger;
    private DelegateCommand? _okCommand;
    private DelegateCommand? _cancelCommand;
    private DelegateCommand? _yesCommand;
    private DelegateCommand? _noCommand;

    protected DialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        AppConfig appConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, objectLifetimeChecker)
    {
        _resourcesHolder = resourcesHolder;
        _logger = logger;

        Observable
            .FromEventPattern(
                h => _resourcesHolder.CultureChanged += h,
                h => _resourcesHolder.CultureChanged -= h)
            .Subscribe(_ => RaisePropertyChanged(nameof(R)))
            .AddTo(Trash);

 #pragma warning disable CS8619
        ViewerFontFamily = appConfig.Data
            .ObserveProperty(x => x.ViewerFontFamily)
            .ToReadOnlyReactiveProperty()
            .AddTo(Trash);
 #pragma warning restore CS8619

        ViewerFontSize = appConfig.Data
            .ObserveProperty(x => x.ViewerFontSize)
            .ToReadOnlyReactiveProperty()
            .AddTo(Trash);

        _logger.Start(GetType().Name);
    }

    public override void Dispose()
    {
        _logger.End(GetType().Name);

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
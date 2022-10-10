using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Foundations;
using Anna.Gui.Views.Windows;
using Anna.UseCase;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;

namespace Anna.Gui;

public class GuiApp : Application
{
    private readonly IServiceProviderContainer? _dic;
    private readonly Action? _onExit;
    private readonly CompositeDisposable _trash = new();
    
    public GuiApp()
    {
    }

    public GuiApp(IServiceProviderContainer dic, Action? onExit)
    {
        _dic = dic;
        _onExit = onExit;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            SetupDesktop(desktop);
    }

    private void SetupDesktop(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _ = _dic ?? throw new NullReferenceException();

        ReactivePropertyScheduler.SetDefault(AvaloniaScheduler.Instance);

        desktop.Exit += async (_, _) =>
        {
            await DispatcherHelper.DoEventsAsync();
            _onExit?.Invoke();
        };

        _dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.Culture)
            .ObserveOnUIDispatcher()
            .Subscribe(x => _dic.GetInstance<ResourcesHolder>().SetCulture(x))
            .AddTo(_trash);

        _dic.GetInstance<App>().Folders.ObserveAddChanged()
            .Subscribe(x =>
            {
                var d = _dic.GetInstance<FolderWindow>();
                d.DataContext = _dic.GetInstance<FolderWindowViewModel, Folder>(x);
                d.Show();
            }).AddTo(_trash);

        var commandLine = CommandLine.Parse(desktop.Args ?? Array.Empty<string>());

        var targetDir = commandLine is not null
            ? commandLine.TargetFolder
            : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        _dic.GetInstance<App>().ShowFolderAsync(targetDir);
    }
}
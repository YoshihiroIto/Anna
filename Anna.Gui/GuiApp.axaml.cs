using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Gui.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleInjector;
using System;
using System.Reactive.Disposables;
using DirectoryWindow=Anna.Gui.Views.DirectoryWindow;

namespace Anna.Gui;

public class GuiApp : Application
{
    public GuiApp Setup(Container dic, Action? onExit)
    {
        _dic = dic;
        _onExit = onExit;

        return this;
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
            await Dispatcher.UIThread.InvokeAsync(() => {}, DispatcherPriority.Normal);

            _onExit?.Invoke();
        };

        _dic.GetInstance<AppConfig>().Data
            .ObserveProperty(x => x.Culture)
            .ObserveOnUIDispatcher()
            .Subscribe(x => _dic.GetInstance<ResourcesHolder>().SetCulture(x))
            .AddTo(_trash);

        _dic.GetInstance<App>().Directories.ObserveAddChanged()
            .Subscribe(x =>
            {
                var d = new DirectoryWindow { DataContext = _dic.GetInstance<DirectoryWindowViewModel>().Setup(x) };
                d.Show();
            }).AddTo(_trash);

        _dic.GetInstance<App>().ShowDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
        _dic.GetInstance<App>().ShowDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    private Container? _dic;
    private Action? _onExit;
    private readonly CompositeDisposable _trash = new();
}
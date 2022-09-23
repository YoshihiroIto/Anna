using Anna.Constants;
using Anna.DomainModel;
using Anna.Gui.ViewModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SimpleInjector;
using System;
using System.Globalization;
using System.Reactive.Disposables;
using System.Threading;
using DirectoryWindow=Anna.Gui.Views.DirectoryWindow;
using MainWindow=Anna.Gui.Views.MainWindow;

namespace Anna.Gui;

public class GuiApp : Application
{
    public GuiApp Setup(Container dic, Action? onMainWindowClosed)
    {
        _dic = dic;
        _onMainWindowClosed = onMainWindowClosed;

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

        desktop.MainWindow = new MainWindow { DataContext = _dic.GetInstance<MainWindowViewModel>() };
        desktop.MainWindow.Closed += (_, _) =>
        {
            desktop.MainWindow = null;
            _dic.GetInstance<App>().CloseAllDirectories();
            _trash.Dispose();

            _onMainWindowClosed?.Invoke();
        };

        _dic.GetInstance<Config>().ConfigData
            .ObserveProperty(x => x.Culture)
            .ObserveOnUIDispatcher()
            .Subscribe(SetCulture)
            .AddTo(_trash);

        _dic.GetInstance<App>().Directories.ObserveAddChanged()
            .Subscribe(x =>
            {
                var d = new DirectoryWindow { DataContext = _dic.GetInstance<DirectoryWindowViewModel>().Setup(x) };
                d.Show();
            }).AddTo(_trash);

        _dic.GetInstance<App>().Directories.CollectionChangedAsObservable()
            .Subscribe(_ =>
            {
                if (_dic.GetInstance<App>().Directories.Count == 0)
                    desktop.MainWindow?.Close();
            }).AddTo(_trash);

        _dic.GetInstance<App>().ShowDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    private static void SetCulture(Cultures culture)
    {
        Strings.Resources.Culture = CultureInfo.GetCultureInfo(culture.ToString());

        Thread.CurrentThread.CurrentCulture = Strings.Resources.Culture;
        Thread.CurrentThread.CurrentUICulture = Strings.Resources.Culture;
    }

    private Container? _dic;
    private Action? _onMainWindowClosed;
    private readonly CompositeDisposable _trash = new();
}
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
using System.Reactive.Disposables;
using DirectoryWindow=Anna.Gui.Views.DirectoryWindow;
using MainWindow=Anna.Gui.Views.MainWindow;

namespace Anna.Gui;

public class GuiApp : Application
{
    public GuiApp Setup(Container dic)
    {
        _dic = dic;

        return this;
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            SetupDesktop(desktop);

        base.OnFrameworkInitializationCompleted();
    }

    private void SetupDesktop(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _ = _dic ?? throw new NullReferenceException();
        
        ReactivePropertyScheduler.SetDefault(AvaloniaScheduler.Instance);
    
        desktop.MainWindow = new MainWindow { DataContext = _dic.GetInstance<MainWindowViewModel>() };
        desktop.MainWindow.Closed += OnClosed;

        _dic.GetInstance<App>().Directories.ObserveAddChanged()
            .Subscribe(x =>
            {
                var d = new DirectoryWindow { DataContext = _dic.GetInstance<DirectoryWindowViewModel>().Setup(x) };
                d.Show();
            }).AddTo(_trash);

        _dic.GetInstance<App>().ShowDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _trash.Dispose();
    }

    private Container? _dic;
    private readonly CompositeDisposable _trash = new();
}
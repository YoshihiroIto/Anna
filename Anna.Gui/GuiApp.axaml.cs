using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.ServiceProvider;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Anna.ViewModels;
using Anna.Views;
using Avalonia.Threading;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.Reactive.Disposables;

namespace Anna;

public class GuiApp : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            Setup(desktop);

        base.OnFrameworkInitializationCompleted();
    }

    private void Setup(IClassicDesktopStyleApplicationLifetime desktop)
    {
        ReactivePropertyScheduler.SetDefault(AvaloniaScheduler.Instance);
    
        _dic.GetInstance<IObjectLifetimeChecker>().Start(s =>
        {
            Debug.WriteLine(s);
            Debugger.Break();
        });// todo:MessageDialog

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
        
        _dic.GetInstance<App>().Clean();
        _dic.GetInstance<IObjectLifetimeChecker>().End();
        _dic.Dispose();
    }

    private readonly CompositeDisposable _trash = new();
    private readonly Container _dic = new();
}
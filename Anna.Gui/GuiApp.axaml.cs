using Anna.DomainModel.Interface;
using Anna.ServiceProvider;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Anna.ViewModels;
using Anna.Views;
using System;
using System.Diagnostics;

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
        {
            _dic.GetInstance<IObjectLifetimeChecker>().Start(s =>
            {
                Debug.WriteLine(s);
                Debugger.Break();
            });// todo:MessageDialog

            desktop.MainWindow = new MainWindow { DataContext = _dic.GetInstance<MainWindowViewModel>() };
            desktop.MainWindow.Closed += (sender, _) => _dic.GetInstance<IObjectLifetimeChecker>().End();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private readonly Container _dic = new();
}
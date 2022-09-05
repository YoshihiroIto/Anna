using Anna.DomainModel.Interface;
using Anna.ServiceProvider;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Anna.ViewModels;
using Anna.Views;
using Avalonia.Xaml.Interactivity;
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
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                _dic.GetInstance<IObjectLifetimeChecker>().Start(s => Debug.WriteLine(s)); // todo:MessageDialog

                desktop.MainWindow = new MainWindow { DataContext = _dic.GetInstance<MainWindowViewModel>() };
                desktop.MainWindow.Closed += (sender, _) => _dic.GetInstance<IObjectLifetimeChecker>().End();

                break;

            default:
                throw new NotImplementedException();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private readonly Container _dic = new();
}
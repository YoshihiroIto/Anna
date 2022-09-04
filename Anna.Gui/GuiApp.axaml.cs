using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Anna.ViewModels;
using Anna.Views;
using System;

namespace Anna
{
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
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainViewModel()
                    };
                    break;
                
                default:
                    throw new NotImplementedException();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
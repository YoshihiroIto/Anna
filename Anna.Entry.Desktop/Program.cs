using Anna.DomainModel;
using Anna.DomainModel.Interfaces;
using Anna.Gui;
using Anna.ServiceProvider;
using System;
using Avalonia;
using SimpleInjector;
using System.IO;
using System.Runtime;
using Directory=System.IO.Directory;

namespace Anna.Desktop;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var commandLine = CommandLine.Parse(args);

        var appConfigFilePath = commandLine is null
            ? CommandLine.DefaultAppConfigFilePath
            : commandLine.AppConfigFilePath;

        // Assembly loading optimization
        var configDir = Path.GetDirectoryName(appConfigFilePath) ??
                        CommandLine.DefaultAppConfigFilePath;
        {
            Directory.CreateDirectory(configDir);
            ProfileOptimization.SetProfileRoot(configDir);
            ProfileOptimization.StartProfile("Startup.Profile");
        }

        var dic = new ServiceProviderContainer(configDir, appConfigFilePath);
        
        dic.GetInstance<ILogger>().Information("Start");

        BuildAvaloniaApp(dic)
            .StartWithClassicDesktopLifetime(args);

        dic.GetInstance<App>().Clean();
        dic.Destroy();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp(Container dic)
        => AppBuilder.Configure(() => new GuiApp().Setup(dic))
            .UsePlatformDetect()
            .LogToTrace();
    
    // for designer
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => new GuiApp())
            .UsePlatformDetect()
            .LogToTrace();
    
}
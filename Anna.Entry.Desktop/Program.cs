using Anna.Gui;
using Anna.ServiceProvider;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;
using System;
using System.Reflection;
using IServiceProvider=Anna.UseCase.IServiceProvider;

namespace Anna.Entry.Desktop;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var dic = DefaultServiceProvider.Create(args);

        BuildAvaloniaApp(dic, null)
            .StartWithClassicDesktopLifetime(args);

        dic.Destroy();
        (App as IDisposable)?.Dispose();
    }

    public static AppBuilder BuildAvaloniaAppForDesktopTests(DefaultServiceProvider dic)
    {
        return BuildAvaloniaApp(dic, dic.Destroy);
    }

    // for designer
    // ReSharper disable once UnusedMember.Local
    private static AppBuilder BuildAvaloniaApp()
    {
        GC.KeepAlive(Assembly.GetAssembly(typeof(Interaction)));
        GC.KeepAlive(Assembly.GetAssembly(typeof(EventTriggerBehavior)));

        return AppBuilder.Configure(() => new GuiApp())
            .UsePlatformDetect()
            .LogToTrace();
    }

    private static AppBuilder BuildAvaloniaApp(IServiceProvider dic, Action? onExit)
        => AppBuilder.Configure(() => new GuiApp(dic, onExit))
            .UsePlatformDetect()
            .LogToTrace();

    private static IClassicDesktopStyleApplicationLifetime? App =>
        (IClassicDesktopStyleApplicationLifetime?) Application.Current?.ApplicationLifetime;
}

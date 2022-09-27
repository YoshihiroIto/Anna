﻿using Anna.Gui;
using Anna.ServiceProvider;
using Anna.UseCase;
using Avalonia;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;
using System;
using System.IO;
using System.Reflection;
using System.Runtime;
using Directory=System.IO.Directory;

namespace Anna.Entry.Desktop;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var dic = CreateServiceProviderContainer(args);

        BuildAvaloniaApp(dic, null)
            .StartWithClassicDesktopLifetime(args);

        dic.Destroy();
    }

    public static AppBuilder BuildAvaloniaAppForDesktopTests(string[] args)
    {
        var dic = CreateServiceProviderContainer(args);

        return BuildAvaloniaApp(dic, () => dic.Destroy());
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

    private static AppBuilder BuildAvaloniaApp(IServiceProviderContainer dic, Action? onExit)
        => AppBuilder.Configure(() => new GuiApp(dic, onExit))
            .UsePlatformDetect()
            .LogToTrace();

    private static DefaultServiceProviderContainer CreateServiceProviderContainer(string[] args)
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

        return new DefaultServiceProviderContainer(configDir, appConfigFilePath);
    }
}
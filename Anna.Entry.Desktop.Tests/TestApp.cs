using Anna.Gui.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Threading;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Anna.Entry.Desktop.Tests;

public class TestApp : IAsyncDisposable 
{
    public MainWindow MainWindow =>
        App.MainWindow as MainWindow ?? throw new NullReferenceException();

    public IClassicDesktopStyleApplicationLifetime App =>
        Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime ??
        throw new NullReferenceException();

    public TestApp(bool isHeadless = true)
    {
        Task.Run(() => StartAsync(isHeadless)).Wait();
    }
    
    public async ValueTask DisposeAsync()
    {
        await EndAsync();
    }

    private async Task StartAsync(bool isHeadless = true)
    {
 #pragma warning disable CS4014
        Task.Run(() =>
        {
            var args = Array.Empty<string>();

            BuildAvaloniaApp(isHeadless, args)
                .StartWithClassicDesktopLifetime(args);
        });
 #pragma warning restore CS4014

        while (IsAvailableMainWindow == false)
            await Task.Delay(TimeSpan.FromMilliseconds(50));
    }

    private Task EndAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() => MainWindow.Close());
    }

    private bool IsAvailableMainWindow =>
        Application.Current?.ApplicationLifetime is not null &&
        App.MainWindow is not null;

    private static AppBuilder BuildAvaloniaApp(bool isHeadless, string[] args)
    {
        var appBuilder = Program.BuildAvaloniaAppForDesktopTests(args);

        if (isHeadless)
            appBuilder.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseCompositor = false });

        return appBuilder;
    }
}
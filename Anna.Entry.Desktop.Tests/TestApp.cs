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
    public MainWindow MainWindow => App.MainWindow as MainWindow ?? throw new NullReferenceException();

    public TestApp(TempDir? configDir = null, bool isHeadless = true)
    {
        if (configDir is null)
        {
            _configDir = new TempDir();
            _useSelfConfigDir = true;
        }
        else
        {
            _configDir = configDir;
        }

        Task.Run(() => StartAsync(isHeadless)).Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await EndAsync();

        if (_useSelfConfigDir)
            _configDir.Dispose();
        
        GC.SuppressFinalize(this);
    }

    private async Task StartAsync(bool isHeadless = true)
    {
 #pragma warning disable CS4014
        Task.Run(() =>
        {
            var args = new[] { "--config", _configDir.AppConfigFilePath };

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

    private IClassicDesktopStyleApplicationLifetime App =>
        Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime ??
        throw new NullReferenceException();

    private static AppBuilder BuildAvaloniaApp(bool isHeadless, string[] args)
    {
        var appBuilder = Program.BuildAvaloniaAppForDesktopTests(args);

        if (isHeadless)
            appBuilder.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseCompositor = false });

        return appBuilder;
    }

    private readonly TempDir _configDir;
    private readonly bool _useSelfConfigDir;
}
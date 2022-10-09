using Anna.Gui.Views.Windows;
using Anna.ServiceProvider;
using Anna.TestFoundation;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Threading;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Anna.Entry.Desktop.Tests;

public class TestApp : IAsyncDisposable
{
 #pragma warning disable CA1822
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public IEnumerable<FolderWindow> FolderWindows => App.Windows.OfType<FolderWindow>();
 #pragma warning restore CA1822

    public DefaultServiceProviderContainer ServiceProviderContainer { get; }

    public TestApp(TempFolder? configFolder = null, string workFolder = "", bool isHeadless = true)
    {
        if (configFolder is null)
        {
            _configFolder = new TempFolder();
            _useSelfConfigFolder = true;
        }
        else
        {
            _configFolder = configFolder;
        }

        var args = new[]
        {
            "--config", _configFolder.AppConfigFilePath, "--target", Path.Combine(_configFolder.RootPath, workFolder)
        };

        ServiceProviderContainer = DefaultServiceProviderContainer.Create(args);

        Task.Run(() => StartAsync(args, isHeadless)).Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await WaitForAllWindowClosedAsync();

        _sync.Wait();
        _sync.Dispose();

        if (_useSelfConfigFolder)
            _configFolder.Dispose();

        Dispatcher.UIThread.Post(() => App.Shutdown());
        (App as IDisposable)?.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task StartAsync(string[] args, bool isHeadless)
    {
 #pragma warning disable CS4014
        Task.Run(() =>
        {
            BuildAvaloniaApp(isHeadless, ServiceProviderContainer)
                .StartWithClassicDesktopLifetime(args);

            _sync.Set();
        });
 #pragma warning restore CS4014

        while (IsAvailableWindows == false)
            await Task.Delay(TimeSpan.FromMilliseconds(50));
    }

    private Task WaitForAllWindowClosedAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var dw in FolderWindows.ToArray())
                dw.Close();
        });
    }

    private static bool IsAvailableWindows =>
        Application.Current?.ApplicationLifetime is not null &&
        App.Windows.Count > 0;

    private static IClassicDesktopStyleApplicationLifetime App =>
        Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime ??
        throw new NullReferenceException();

    private static AppBuilder BuildAvaloniaApp(bool isHeadless, DefaultServiceProviderContainer dic)
    {
        var appBuilder = Program.BuildAvaloniaAppForDesktopTests(dic);

        if (isHeadless)
            appBuilder.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseCompositor = false });

        return appBuilder;
    }

    private readonly TempFolder _configFolder;
    private readonly bool _useSelfConfigFolder;
    private readonly ManualResetEventSlim _sync = new();
}
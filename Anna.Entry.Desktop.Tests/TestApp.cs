using Anna.Foundation;
using Anna.Gui.Views.Windows;
using Anna.ServiceProvider;
using Anna.TestFoundation;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Threading;
using Jewelry.Memory;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Anna.Entry.Desktop.Tests;

public sealed class TestApp : IAsyncDisposable
{
 #pragma warning disable CA1822
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public IEnumerable<FolderWindow> FolderWindows => App.Windows.OfType<FolderWindow>();
 #pragma warning restore CA1822

    public DefaultServiceProvider Dic { get; }

    public TestApp(TempFolder? configFolder = null, string workFolder = "", bool isHeadless = false)
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

        Dic = DefaultServiceProvider.Create(args);

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
    }

    private async Task StartAsync(string[] args, bool isHeadless)
    {
        Task.Run(() =>
        {
            BuildAvaloniaApp(isHeadless, Dic)
                .StartWithClassicDesktopLifetime(args);

            _sync.Set();
        }).Forget();

        while (IsAvailableWindows == false)
            await Task.Delay(TimeSpan.FromMilliseconds(50));
    }

    private Task WaitForAllWindowClosedAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            using var folderWindows = FolderWindows.ToPooledArray();
            
            foreach (var dw in folderWindows.AsSpan())
                dw.Close();
        });
    }

    private static bool IsAvailableWindows =>
        Application.Current?.ApplicationLifetime is not null &&
        App.Windows.Count > 0;

    private static IClassicDesktopStyleApplicationLifetime App =>
        Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime ??
        throw new NullReferenceException();

    private static AppBuilder BuildAvaloniaApp(bool isHeadless, DefaultServiceProvider dic)
    {
        var appBuilder = Program.BuildAvaloniaAppForDesktopTests(dic);

         if (isHeadless)
            appBuilder.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseCompositor = false, UseHeadlessDrawing = false});
            
        return appBuilder;
    }

    private readonly TempFolder _configFolder;
    private readonly bool _useSelfConfigFolder;
    private readonly ManualResetEventSlim _sync = new();
}
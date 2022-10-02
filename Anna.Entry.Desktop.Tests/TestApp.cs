using Anna.Gui.Views.Windows;
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
    public IEnumerable<DirectoryWindow> DirectoryWindows => App.Windows.OfType<DirectoryWindow>();
 #pragma warning restore CA1822

    public TestApp(TempDir? configDir = null, string workDir = "",  bool isHeadless = true)
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

        Task.Run(() => StartAsync(Path.Combine(_configDir.RootPath, workDir), isHeadless)).Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await WaitForAllWindowClosedAsync();

        await _sema.WaitAsync();
        _sema.Dispose();

        if (_useSelfConfigDir)
            _configDir.Dispose();

        Dispatcher.UIThread.Post(() => App.Shutdown());
        (App as IDisposable)?.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task StartAsync(string targetDir, bool isHeadless)
    {
        await _sema.WaitAsync();

 #pragma warning disable CS4014
        Task.Run(() =>
        {
            var args = new[] { "--config", _configDir.AppConfigFilePath, "--target", targetDir };

            BuildAvaloniaApp(isHeadless, args)
                .StartWithClassicDesktopLifetime(args);

            _sema.Release();
        });
 #pragma warning restore CS4014

        while (IsAvailableWindows == false)
            await Task.Delay(TimeSpan.FromMilliseconds(50));
    }

    private Task WaitForAllWindowClosedAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var dw in DirectoryWindows.ToArray())
                dw.Close();
        });
    }

    private static bool IsAvailableWindows =>
        Application.Current?.ApplicationLifetime is not null &&
        App.Windows.Count > 0;

    private static IClassicDesktopStyleApplicationLifetime App =>
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
    private readonly SemaphoreSlim _sema = new(1, 1);
}
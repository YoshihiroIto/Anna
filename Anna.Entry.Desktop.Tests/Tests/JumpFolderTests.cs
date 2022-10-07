using Anna.DomainModel.Config;
using Anna.Gui.Views.Windows;
using Anna.Repository;
using Avalonia.Input;
using Avalonia.Threading;
using System.Text.Json;
using Xunit;

namespace Anna.Entry.Desktop.Tests.Tests;

[Collection("Desktop test collection")]
public class JumpFolderTests : IDisposable
{
    public JumpFolderTests(DesktopTestFixture fixture)
    {
        _fixture = fixture;

        _fixture.ConfigFolder.CreateWorkFolder();
    }

    public void Dispose()
    {
        _fixture.Teardown();
    }

    private readonly DesktopTestFixture _fixture;

    [Fact]
    public async Task Open_escape()
    {
        var configFolder = _fixture.ConfigFolder;

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.J);
            await w.PressKeyAsync(Key.Escape);

            return (w.DataContext as FolderWindowViewModel)?.Model ?? throw new NullReferenceException();
        });

        await Task.Delay(100);

        Assert.Equal(configFolder.WorkPath, model.Path);
    }

    [Fact]
    public async Task Select_empty()
    {
        var configFolder = _fixture.ConfigFolder;

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.J);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as FolderWindowViewModel)?.Model ?? throw new NullReferenceException();
        });

        await Task.Delay(100);

        Assert.Equal(configFolder.WorkPath, model.Path);
    }

    [Fact]
    public async Task Select_folder()
    {
        var configFolder = _fixture.ConfigFolder;
        configFolder.CreateFolder("FolderA");

        var c = new JumpFolderConfigData();
        c.SetDefault();
        var a = c.Paths.First(x => x.Key == Key.A);
        a.Path = Path.Combine(configFolder.WorkPath, "FolderA");

        var json = JsonSerializer.Serialize(c, FileSystemObjectSerializer.Options);
        await File.WriteAllTextAsync(configFolder.JumpFolderConfigFilePath, json);

        _fixture.App.ServiceProviderContainer.GetInstance<JumpFolderConfig>().Load();

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.J);
            await w.PressKeyAsync(Key.A);

            return (w.DataContext as FolderWindowViewModel)?.Model ?? throw new NullReferenceException();
        });

        await Task.Delay(100);

        Assert.Equal(a.Path, model.Path);
    }

    [Fact]
    public async Task Delete_folder()
    {
        var configFolder = _fixture.ConfigFolder;
        configFolder.CreateFolder("FolderA");
        
        var c = new JumpFolderConfigData();
        c.SetDefault();
        var f1 = c.Paths.First(x => x.Key == Key.F1);
        f1.Path = "ABC";

        var json = JsonSerializer.Serialize(c, FileSystemObjectSerializer.Options);
        await File.WriteAllTextAsync(configFolder.JumpFolderConfigFilePath, json);

        _fixture.App.ServiceProviderContainer.GetInstance<JumpFolderConfig>().Load();

        await Task.Delay(100);

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.J);
            await w.PressKeyAsync(Key.Delete);
            await w.PressKeyAsync(Key.Enter);       // Confirmation dialog
            await w.PressKeyAsync(Key.Escape);
        });

        await Task.Delay(100);

        var afterF1 = _fixture.App.ServiceProviderContainer.GetInstance<JumpFolderConfig>().Data.Paths
            .First(x => x.Key == Key.F1);
        
        Assert.Equal("", afterF1.Path);
    }
}
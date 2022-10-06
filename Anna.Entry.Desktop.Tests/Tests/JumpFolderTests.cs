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
        _fixture.ConfigFolder.DeleteWorkFolder();
    }

    private readonly DesktopTestFixture _fixture;

    [Fact]
    public async Task Dialog_open_escape()
    {
        var configFolder = _fixture.ConfigFolder;
        var app = _fixture.App;

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.FolderWindows.First();

            await w.PressKeyAsync(Key.J);
            await w.PressKeyAsync(Key.Escape);

            return (w.DataContext as FolderWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(configFolder.WorkPath, model.Path);
    }

    [Fact]
    public async Task Dialog_select_empty()
    {
        var configFolder = _fixture.ConfigFolder;
        var app = _fixture.App;

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.FolderWindows.First();

            await w.PressKeyAsync(Key.J);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as FolderWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(configFolder.WorkPath, model.Path);
    }

#if false
    [Fact]
    public async Task Dialog_select_folder()
    {
        var configFolder = _fixture.ConfigFolder;
        configFolder.CreateFolder("FolderA");

        var c = new JumpFolderConfigData();
        c.SetDefault();
        var a = c.Paths.First(x => x.Key == Key.A);
        a.Path = Path.Combine(configFolder.WorkPath, "FolderA");

        var json = JsonSerializer.Serialize(c, FileSystemObjectSerializer.Options);

        var configPath =
            Path.Combine(Path.GetDirectoryName(configFolder.AppConfigFilePath)!, JumpFolderConfig.Filename);

        await File.WriteAllTextAsync(configPath, json);

        var app = _fixture.App;

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.FolderWindows.First();

            await w.PressKeyAsync(Key.J);
            await w.PressKeyAsync(Key.A);

            return (w.DataContext as FolderWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(a.Path, model.Path);
    }
#endif
}
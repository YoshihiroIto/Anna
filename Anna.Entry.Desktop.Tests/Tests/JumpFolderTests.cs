using Anna.DomainModel.Config;
using Anna.Service.Repository;
using Anna.Service.Services;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System.Text.Json;
using Xunit;

namespace Anna.Entry.Desktop.Tests.Tests;

[Collection("Desktop test collection")]
public sealed class JumpFolderTests : IDisposable
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

            await Task.Delay(100);
            await w.PressKeyAsync(Key.J);
            await Task.Delay(100);
            await w.PressKeyAsync(Key.Escape);
            await Task.Delay(100);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(configFolder.WorkPath, model.Folder.Path);
    }

    [Fact]
    public async Task Select_empty()
    {
        var configFolder = _fixture.ConfigFolder;

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await Task.Delay(100);
            await w.PressKeyAsync(Key.J);
            await Task.Delay(100);
            await w.PressKeyAsync(Key.Enter);
            await Task.Delay(100);

            await Task.Delay(500);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(configFolder.WorkPath, model.Folder.Path);
    }

    [Fact]
    public async Task Select_folder()
    {
        var configFolder = _fixture.ConfigFolder;
        configFolder.CreateFolder("FolderA");

        var c = new JumpFolderConfigData();
        c.SetDefault(_fixture.App.Dic.GetInstance<IDefaultValueService>());
        var a = c.Paths.First(x => x.Key == Key.A);
        a.Path = Path.Combine(configFolder.WorkPath, "FolderA");

        var json = JsonSerializer.Serialize(c, FileSystemObjectSerializer.Options);
        await File.WriteAllTextAsync(configFolder.JumpFolderConfigFilePath, json);

        _fixture.App.Dic.GetInstance<JumpFolderConfig>().Load();

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await Task.Delay(100);
            await w.PressKeyAsync(Key.J);
            await Task.Delay(100);
            await w.PressKeyAsync(Key.A);
            await Task.Delay(100);

            await Task.Delay(500);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(a.Path, model.Folder.Path);
    }

    [Fact]
    public async Task Delete_folder()
    {
        var configFolder = _fixture.ConfigFolder;

        var c = new JumpFolderConfigData();
        c.SetDefault(_fixture.App.Dic.GetInstance<IDefaultValueService>());
        var f1 = c.Paths.First(x => x.Key == Key.F1);
        f1.Path = "ABC";

        var json = JsonSerializer.Serialize(c, FileSystemObjectSerializer.Options);
        await File.WriteAllTextAsync(configFolder.JumpFolderConfigFilePath, json);

        _fixture.App.Dic.GetInstance<JumpFolderConfig>().Load();

        await Task.Delay(100);

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();
            await Task.Delay(100);

            await w.PressKeyAsync(Key.J);
            await Task.Delay(100);
            await w.PressKeyAsync(Key.Delete);
            await Task.Delay(100);
            await w.PressKeyAsync(Key.Enter);// Confirmation dialog
            await Task.Delay(100);
            await w.PressKeyAsync(Key.Escape);
        });

        await Task.Delay(100);

        var afterF1 = _fixture.App.Dic.GetInstance<JumpFolderConfig>().Data.Paths
            .First(x => x.Key == Key.F1);

        Assert.Equal("", afterF1.Path);
    }

    [Fact]
    public async Task Edit_folder()
    {
        await Task.Delay(100);

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await Task.Delay(100);
            await w.PressKeyAsync(Key.J);
            await Task.Delay(100);
            await w.PressKeyAsync(Key.Insert);
            await Task.Delay(100);

            var textBox = FocusManager.Instance?.Current as TextBox ?? throw new NullReferenceException();
            textBox.Text = "123abc";

            await Task.Delay(100);
            await w.PressKeyAsync(Key.Enter);
            await Task.Delay(100);
            await w.PressKeyAsync(Key.Escape);
            await Task.Delay(100);
        });

        await Task.Delay(100);

        var afterF1 = _fixture.App.Dic.GetInstance<JumpFolderConfig>().Data.Paths
            .First(x => x.Key == Key.F1);

        Assert.Equal("123abc", afterF1.Path);
    }
}
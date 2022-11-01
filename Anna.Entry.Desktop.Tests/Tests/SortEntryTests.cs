using Avalonia.Input;
using Avalonia.Threading;
using Xunit;

namespace Anna.Entry.Desktop.Tests.Tests;

[Collection("Desktop test collection")]
public sealed class SortEntryTests : IDisposable
{
    public SortEntryTests(DesktopTestFixture fixture)
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
    public async Task Name_Ascending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("002.dat");
        configFolder.CreateFile("003.dat");
        configFolder.CreateFile("001.dat");
        configFolder.CreateFolder("dir2");
        configFolder.CreateFolder("dir3");
        configFolder.CreateFolder("dir1");

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(7, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("dir1", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("dir2", model.Folder.Entries[2].NameWithExtension);
        Assert.Equal("dir3", model.Folder.Entries[3].NameWithExtension);
        Assert.Equal("001.dat", model.Folder.Entries[4].NameWithExtension);
        Assert.Equal("002.dat", model.Folder.Entries[5].NameWithExtension);
        Assert.Equal("003.dat", model.Folder.Entries[6].NameWithExtension);
    }

    [Fact]
    public async Task Name_Descending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("002.dat");
        configFolder.CreateFile("003.dat");
        configFolder.CreateFile("001.dat");
        configFolder.CreateFolder("dir2");
        configFolder.CreateFolder("dir3");
        configFolder.CreateFolder("dir1");

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(7, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("dir3", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("dir2", model.Folder.Entries[2].NameWithExtension);
        Assert.Equal("dir1", model.Folder.Entries[3].NameWithExtension);
        Assert.Equal("003.dat", model.Folder.Entries[4].NameWithExtension);
        Assert.Equal("002.dat", model.Folder.Entries[5].NameWithExtension);
        Assert.Equal("001.dat", model.Folder.Entries[6].NameWithExtension);
    }

    [Fact]
    public async Task Extension_Ascending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("001.c");
        configFolder.CreateFile("002.b");
        configFolder.CreateFile("003.a");

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(4, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("003.a", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Folder.Entries[2].NameWithExtension);
        Assert.Equal("001.c", model.Folder.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Extension_Descending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("001.c");
        configFolder.CreateFile("002.b");
        configFolder.CreateFile("003.a");

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(4, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("001.c", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Folder.Entries[2].NameWithExtension);
        Assert.Equal("003.a", model.Folder.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Timestamp_Ascending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("003.a", text: "A");
        await Task.Delay(100);
        configFolder.CreateFile("002.b", text: "AA");
        await Task.Delay(100);
        configFolder.CreateFile("001.c", text: "AAA");

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(4, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("003.a", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Folder.Entries[2].NameWithExtension);
        Assert.Equal("001.c", model.Folder.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Timestamp_Descending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("003.a", text: "A");
        await Task.Delay(100);
        configFolder.CreateFile("002.b", text: "AA");
        await Task.Delay(100);
        configFolder.CreateFile("001.c", text: "AAA");

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(4, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("001.c", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Folder.Entries[2].NameWithExtension);
        Assert.Equal("003.a", model.Folder.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Size_Ascending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("001.c", text: "AAA");
        configFolder.CreateFile("002.b", text: "AA");
        configFolder.CreateFile("003.a", text: "A");

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(4, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("003.a", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Folder.Entries[2].NameWithExtension);
        Assert.Equal("001.c", model.Folder.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Size_Descending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("001.c", text: "AAA");
        configFolder.CreateFile("002.b", text: "AA");
        configFolder.CreateFile("003.a", text: "A");

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(4, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("001.c", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Folder.Entries[2].NameWithExtension);
        Assert.Equal("003.a", model.Folder.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Attributes_Ascending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("001.b");
        configFolder.CreateFile("002.c", attributes: FileAttributes.Temporary);

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(3, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("001.b", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("002.c", model.Folder.Entries[2].NameWithExtension);
    }

    [Fact]
    public async Task Attributes_Descending()
    {
        var configFolder = _fixture.ConfigFolder;

        configFolder.CreateFile("001.b");
        configFolder.CreateFile("002.c", attributes: FileAttributes.Temporary);

        await Task.Delay(100);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return w.ViewModel.Model;
        });

        await Task.Delay(100);

        Assert.Equal(3, model.Folder.Entries.Count);
        Assert.Equal("..", model.Folder.Entries[0].NameWithExtension);
        Assert.Equal("002.c", model.Folder.Entries[1].NameWithExtension);
        Assert.Equal("001.b", model.Folder.Entries[2].NameWithExtension);
    }
}
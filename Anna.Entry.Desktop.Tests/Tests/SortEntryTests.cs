using Anna.Gui.Views.Windows;
using Avalonia.Input;
using Avalonia.Threading;
using Xunit;

namespace Anna.Entry.Desktop.Tests.Tests;

[Collection("Desktop test collection")]
public class SortEntryTests : IDisposable
{
    public SortEntryTests(DesktopTestFixture fixture)
    {
        _fixture = fixture;

        _fixture.ConfigDir.CreateWorkDirectory();
    }

    public void Dispose()
    {
        _fixture.ConfigDir.DeleteWorkDirectory();
    }

    private readonly DesktopTestFixture _fixture;

    [Fact]
    public async Task Name_Ascending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("002.dat");
        configDir.CreateFile("003.dat");
        configDir.CreateFile("001.dat");
        configDir.CreateDirectory("dir2");
        configDir.CreateDirectory("dir3");
        configDir.CreateDirectory("dir1");

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(7, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("dir1", model.Entries[1].NameWithExtension);
        Assert.Equal("dir2", model.Entries[2].NameWithExtension);
        Assert.Equal("dir3", model.Entries[3].NameWithExtension);
        Assert.Equal("001.dat", model.Entries[4].NameWithExtension);
        Assert.Equal("002.dat", model.Entries[5].NameWithExtension);
        Assert.Equal("003.dat", model.Entries[6].NameWithExtension);
    }

    [Fact]
    public async Task Name_Descending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("002.dat");
        configDir.CreateFile("003.dat");
        configDir.CreateFile("001.dat");
        configDir.CreateDirectory("dir2");
        configDir.CreateDirectory("dir3");
        configDir.CreateDirectory("dir1");

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(7, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("dir3", model.Entries[1].NameWithExtension);
        Assert.Equal("dir2", model.Entries[2].NameWithExtension);
        Assert.Equal("dir1", model.Entries[3].NameWithExtension);
        Assert.Equal("003.dat", model.Entries[4].NameWithExtension);
        Assert.Equal("002.dat", model.Entries[5].NameWithExtension);
        Assert.Equal("001.dat", model.Entries[6].NameWithExtension);
    }

    [Fact]
    public async Task Extension_Ascending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("001.c");
        configDir.CreateFile("002.b");
        configDir.CreateFile("003.a");

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(4, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("003.a", model.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Entries[2].NameWithExtension);
        Assert.Equal("001.c", model.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Extension_Descending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("001.c");
        configDir.CreateFile("002.b");
        configDir.CreateFile("003.a");

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(4, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("001.c", model.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Entries[2].NameWithExtension);
        Assert.Equal("003.a", model.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Size_Ascending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("001.c", text: "AAA");
        configDir.CreateFile("002.b", text: "AA");
        configDir.CreateFile("003.a", text: "A");

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(4, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("003.a", model.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Entries[2].NameWithExtension);
        Assert.Equal("001.c", model.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Size_Descending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("001.c", text: "AAA");
        configDir.CreateFile("002.b", text: "AA");
        configDir.CreateFile("003.a", text: "A");

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(4, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("001.c", model.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Entries[2].NameWithExtension);
        Assert.Equal("003.a", model.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Timestamp_Ascending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("003.a", text: "A");
        await Task.Delay(100);
        configDir.CreateFile("002.b", text: "AA");
        await Task.Delay(100);
        configDir.CreateFile("001.c", text: "AAA");

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(4, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("003.a", model.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Entries[2].NameWithExtension);
        Assert.Equal("001.c", model.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Timestamp_Descending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("003.a", text: "A");
        await Task.Delay(100);
        configDir.CreateFile("002.b", text: "AA");
        await Task.Delay(100);
        configDir.CreateFile("001.c", text: "AAA");

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(4, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("001.c", model.Entries[1].NameWithExtension);
        Assert.Equal("002.b", model.Entries[2].NameWithExtension);
        Assert.Equal("003.a", model.Entries[3].NameWithExtension);
    }

    [Fact]
    public async Task Attributes_Ascending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("001.b");
        configDir.CreateFile("002.c", attributes: FileAttributes.Temporary);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(3, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("001.b", model.Entries[1].NameWithExtension);
        Assert.Equal("002.c", model.Entries[2].NameWithExtension);
    }

    [Fact]
    public async Task Attributes_Descending()
    {
        var configDir = _fixture.ConfigDir;
        var app = _fixture.App;

        configDir.CreateFile("001.b");
        configDir.CreateFile("002.c", attributes: FileAttributes.Temporary);

        var model = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = app.DirectoryWindows.First();

            await w.PressKeyAsync(Key.S);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Down);
            await w.PressKeyAsync(Key.Enter);
            await w.PressKeyAsync(Key.Right);
            await w.PressKeyAsync(Key.Enter);

            return (w.DataContext as DirectoryWindowViewModel)?.Model;
        });

        _ = model ?? throw new NullReferenceException();

        Assert.Equal(3, model.Entries.Count);
        Assert.Equal("..", model.Entries[0].NameWithExtension);
        Assert.Equal("002.c", model.Entries[1].NameWithExtension);
        Assert.Equal("001.b", model.Entries[2].NameWithExtension);
    }
}
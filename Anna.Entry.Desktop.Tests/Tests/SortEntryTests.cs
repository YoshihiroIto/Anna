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
}
using Anna.DomainModel.Config;
using Avalonia.Input;
using Avalonia.Threading;
using Xunit;

namespace Anna.Entry.Desktop.Tests.Tests;

[Collection("Desktop test collection")]
public sealed class ListModeTests : IDisposable
{
    public ListModeTests(DesktopTestFixture fixture)
    {
        _fixture = fixture;

        _fixture.ConfigFolder.CreateWorkFolder();
    }

    public void Dispose()
    {
        _fixture.Teardown();
    }

    private readonly DesktopTestFixture _fixture;

    [Theory]
    [InlineData(Key.D1, 0u)]
    [InlineData(Key.D2, 1u)]
    [InlineData(Key.D3, 2u)]
    [InlineData(Key.D4, 3u)]
    [InlineData(Key.D5, 4u)]
    public async Task LastListMode(Key key, uint mode)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var w = _fixture.App.FolderWindows.First();
            
            await w.PressKeyAsync(key, RawInputModifiers.Shift);
            await Task.Delay(50);
            Assert.Equal(mode, w.ViewModel.Dic.GetInstance<AppConfig>().Data.LastListMode );
        });
    }
}
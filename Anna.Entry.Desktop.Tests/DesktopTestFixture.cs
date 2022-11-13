using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.Service.Repository;
using Anna.Service.Services;
using Anna.TestFoundation;
using System.Text.Json;
using Xunit;

namespace Anna.Entry.Desktop.Tests;

public sealed class DesktopTestFixture : IDisposable
{
    public TempFolder ConfigFolder { get; }
    public TestApp App { get; }

    public DesktopTestFixture()
    {
        var workDir = "test";

        ConfigFolder = new TempFolder(workDir);
        ConfigFolder.CreateWorkFolder();

        App = new TestApp(ConfigFolder, workDir);
    }

    public void Dispose()
    {
        App.DisposeAsync().AsTask().Wait();
        ConfigFolder.Dispose();
    }

    public void Teardown()
    {
        App.Dic.GetInstance<App>().Folders[0].Path = ConfigFolder.WorkPath;
        ConfigFolder.DeleteWorkFolder();

        {
            var c = new JumpFolderConfigData();
            c.SetDefault(App.Dic.GetInstance<IDefaultValueService>());

            var json = JsonSerializer.Serialize(c, FileSystemObjectSerializer.Options);
            File.WriteAllText(ConfigFolder.JumpFolderConfigFilePath, json);
        }
    }
}

[CollectionDefinition("Desktop test collection")]
public sealed class DesktopTestCollection : ICollectionFixture<DesktopTestFixture>
{
}
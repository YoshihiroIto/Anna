using Anna.DomainModel;
using Anna.TestFoundation;
using Xunit;

namespace Anna.Entry.Desktop.Tests;

public class DesktopTestFixture : IDisposable
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

    public void Teardown ()
    {
        App.ServiceProviderContainer.GetInstance<App>().Folders[0].Path = ConfigFolder.WorkPath;
        ConfigFolder.DeleteWorkFolder();
    }
}

[CollectionDefinition("Desktop test collection")]
public class DesktopTestCollection : ICollectionFixture<DesktopTestFixture>
{
}
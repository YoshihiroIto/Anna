using Anna.TestFoundation;
using Xunit;

namespace Anna.Entry.Desktop.Tests;

public class DesktopTestFixture : IDisposable
{
    public TempDir ConfigDir { get; }
    public TestApp App { get; }

    public DesktopTestFixture()
    {
        var workDir = "test";
        
        ConfigDir = new TempDir(workDir);
        ConfigDir.CreateWorkDirectory();
        
        App = new TestApp(ConfigDir, workDir);
    }

    public void Dispose()
    {
        App.DisposeAsync().AsTask().Wait();
        ConfigDir.Dispose();
    }
}

[CollectionDefinition("Desktop test collection")]
public class DesktopTestCollection : ICollectionFixture<DesktopTestFixture>
{
}
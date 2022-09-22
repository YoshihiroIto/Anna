using Xunit;

namespace Anna.Entry.Desktop.Tests.Smoke;

public class SmokeTests
{
    [Fact]
    public async Task Start_and_finish_successfully()
    {
        using var configDir = new TempDir();
        {
            await using var app = new TestApp(configDir);
        }

        var lines = configDir.ReadLogLines();
        Assert.Contains("Start [Application]", lines.First());
        Assert.Contains("End [Application]", lines.Last());

        foreach (var line in lines)
        {
            Assert.DoesNotContain("WARN", line);
            Assert.DoesNotContain("EROR", line);
            Assert.DoesNotContain("FATL", line);
        }
    }
}
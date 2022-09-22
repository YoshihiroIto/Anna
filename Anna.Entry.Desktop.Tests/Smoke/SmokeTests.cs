using Xunit;

namespace Anna.Entry.Desktop.Tests.Smoke
{
    public class SmokeTests
    {
        [Fact]
        public async Task Basic()
        {
            await using var app = new TestApp();
        }
    }
}
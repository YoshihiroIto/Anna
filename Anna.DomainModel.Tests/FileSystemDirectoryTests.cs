using Anna.TestFoundation;
using Xunit;

namespace Anna.DomainModel.Tests;

public class FileSystemDirectoryTests : IDisposable
{
    private readonly TestServiceProviderContainer _dic = new();
    
    [Fact]
    public void Start_and_finish_successfully()
    {
         using var directory = _dic.GetInstance<DomainModelOperator>().CreateDirectory(".");
    }
    
    public void Dispose()
    {
        _dic.Dispose();
    }
}
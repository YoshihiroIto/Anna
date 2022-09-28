using Anna.UseCase;

namespace Anna.DomainModel.FileSystem;

public class DirectoryService : IDirectoryServiceUseCase
{
    public bool IsAccessible(string path)
    {
        try
        {
            _ = System.IO.Directory.EnumerateDirectories(path).FirstOrDefault();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
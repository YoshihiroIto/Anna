using Anna.UseCase;

namespace Anna.DomainModel.FileSystem;

public class FolderService : IFolderServiceUseCase
{
    public bool IsAccessible(string path)
    {
        try
        {
            _ = Directory.EnumerateDirectories(path).FirstOrDefault();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
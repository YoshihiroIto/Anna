using Anna.UseCase;

namespace Anna.DomainModel.FileSystem;

public class FolderService : IFolderServiceUseCase
{
    public bool IsAccessible(string path)
    {
        FileStream? stream = null;

        try
        {
            if (File.Exists(path))
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            else
                _ = Directory.EnumerateDirectories(path).FirstOrDefault();

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            stream?.Dispose();
        }
    }
}
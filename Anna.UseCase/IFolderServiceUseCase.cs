namespace Anna.UseCase;

public interface IFolderServiceUseCase
{
    bool IsAccessible(string path);
}
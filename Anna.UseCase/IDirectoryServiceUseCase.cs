namespace Anna.UseCase;

public interface IDirectoryServiceUseCase
{
    bool IsAccessible(string path);
}
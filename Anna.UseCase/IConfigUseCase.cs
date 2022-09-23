namespace Anna.UseCase;

public interface IConfigUseCase
{
    ValueTask LoadAsync();
    ValueTask SaveAsync();
}
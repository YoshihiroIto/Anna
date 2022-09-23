using Anna.Constants;

namespace Anna.UseCase;

public interface IObjectSerializerUseCase
{
    ValueTask<(T obj, ResultCode code)> ReadAsync<T>(string path, Func<T> defaultGenerator);
    
    ValueTask<ResultCode> WriteAsync<T>(string path, T obj);
}
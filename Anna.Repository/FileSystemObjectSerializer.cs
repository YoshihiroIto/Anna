using Anna.Constants;
using Anna.UseCase;
using System.Text.Json;

namespace Anna.Repository;

public class FileSystemObjectSerializer : IObjectSerializerUseCase
{
    private readonly ILoggerUseCase _logger;

    public FileSystemObjectSerializer(ILoggerUseCase logger)
    {
        _logger = logger;
    }

    public async ValueTask<(T obj, ResultCode code)> ReadAsync<T>(string path, Func<T> defaultGenerator)
    {
        try
        {
            var json = await File.ReadAllTextAsync(path);

            var obj = JsonSerializer.Deserialize<T>(json);

            if (obj is null)
                throw new JsonException();

            return (obj, ResultCode.Ok);
        }
        catch (Exception e)
        {
            _logger.Error($"FileSystemObjectReader.ReadAsync: {e.Message}");
        }

        return (defaultGenerator(), ResultCode.Error);
    }
    
    public async ValueTask<ResultCode> WriteAsync<T>(T obj, string path)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj);

            await File.WriteAllTextAsync(path, json);

            return ResultCode.Ok;
        }
        catch (Exception e)
        {
            _logger.Error($"FileSystemObjectWriter.WriteAsync: {e.Message}");
        }

        return ResultCode.Error;
    }
}
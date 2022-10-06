using Anna.Constants;
using Anna.UseCase;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anna.Repository;

public class FileSystemObjectSerializer : IObjectSerializerUseCase
{
    public FileSystemObjectSerializer(ILoggerUseCase logger)
    {
        _logger = logger;
    }

    public async ValueTask<(T obj, ResultCode code)> ReadAsync<T>(string path, Func<T> defaultGenerator)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);

                var obj = JsonSerializer.Deserialize<T>(json, Options);

                if (obj is null)
                    throw new JsonException();

                return (obj, ResultCode.Ok);
            }
        }
        catch (Exception e)
        {
            _logger.Error($"FileSystemObjectReader.ReadAsync: {e.Message}");
        }

        return (defaultGenerator(), ResultCode.Error);
    }
    public (T obj, ResultCode code) Read<T>(string path, Func<T> defaultGenerator)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);

                var obj = JsonSerializer.Deserialize<T>(json, Options);

                if (obj is null)
                    throw new JsonException();

                return (obj, ResultCode.Ok);
            }
        }
        catch (Exception e)
        {
            _logger.Error($"FileSystemObjectReader.ReadAsync: {e.Message}");
        }

        return (defaultGenerator(), ResultCode.Error);
    }

    public async ValueTask<ResultCode> WriteAsync<T>(string path, T obj)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj, Options);

            await File.WriteAllTextAsync(path, json);

            return ResultCode.Ok;
        }
        catch (Exception e)
        {
            _logger.Error($"FileSystemObjectWriter.WriteAsync: {e.Message}");
        }

        return ResultCode.Error;
    }
    public ResultCode Write<T>(string path, T obj)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj, Options);

            File.WriteAllText(path, json);

            return ResultCode.Ok;
        }
        catch (Exception e)
        {
            _logger.Error($"FileSystemObjectWriter.WriteAsync: {e.Message}");
        }

        return ResultCode.Error;
    }

    private readonly ILoggerUseCase _logger;

    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true, Converters = { new JsonStringEnumConverter() }
    };
}
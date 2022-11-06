using Anna.Constants;
using Anna.Service.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anna.Service.Repository;

public sealed class FileSystemObjectSerializer : IObjectSerializerService
{
    private readonly ILoggerService _logger;

    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true, Converters = { new JsonStringEnumConverter() }
    };
    
    public FileSystemObjectSerializer(ILoggerService logger)
    {
        _logger = logger;
    }

    public async ValueTask<(T obj, ResultCode code)> ReadAsync<T>(string path, Func<T> defaultGenerator)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);

                var obj = JsonSerializer.Deserialize<T>(json, Options);

                if (obj is null)
                    throw new JsonException();

                return (obj, ResultCode.Ok);
            }
        }
        catch (Exception e)
        {
            _logger.Error($"{nameof(FileSystemObjectSerializer)}.{nameof(ReadAsync)}: {e.Message}");
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
            _logger.Error($"{nameof(FileSystemObjectSerializer)}.{nameof(Read)}: {e.Message}");
        }

        return (defaultGenerator(), ResultCode.Error);
    }

    public async ValueTask<ResultCode> WriteAsync<T>(string path, T obj)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj, Options);

            await File.WriteAllTextAsync(path, json).ConfigureAwait(false);

            return ResultCode.Ok;
        }
        catch (Exception e)
        {
            _logger.Error($"{nameof(FileSystemObjectSerializer)}.{nameof(WriteAsync)}: {e.Message}");
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
            _logger.Error($"{nameof(FileSystemObjectSerializer)}.{nameof(Write)}: {e.Message}");
        }

        return ResultCode.Error;
    }
}
using Anna.Service.Services;
using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace Anna.Service.Clipboard;

public sealed class ClipboardService : IClipboardService
{
    private static IClipboard AppClipboard => Application.Current?.Clipboard ?? throw new NullReferenceException();

    public async ValueTask SetFilesAsync(IEnumerable<string> filePaths)
    {
        var filePathsArray = filePaths.ToArray();
        if (filePathsArray.Length == 0)
            return;
        
        var dataObject = new DataObject();
        dataObject.Set(DataFormats.FileNames, filePathsArray);
        dataObject.Set(DataFormats.Text, string.Join('\n', filePathsArray.Select(Path.GetFileName)));

        await AppClipboard.SetDataObjectAsync(dataObject);
    }

    public async ValueTask<IEnumerable<string>> GetFilesAsync()
    {
        return await AppClipboard.GetDataAsync(DataFormats.FileNames) as IEnumerable<string> ?? Enumerable.Empty<string>();
    }

    public async ValueTask SetTextAsync(string text)
    {
        if (text == "")
            return;
        
        var dataObject = new DataObject();
        dataObject.Set(DataFormats.Text, text);

        await AppClipboard.SetDataObjectAsync(dataObject);
    }
}
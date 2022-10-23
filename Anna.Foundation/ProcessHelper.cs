using System.Diagnostics;

namespace Anna.Foundation;

public static class ProcessHelper
{
    public static void Execute(string command, string? arguments = null)
    {
        if (arguments is null)
            Process.Start(command);
        else
            Process.Start(command, arguments);
    }
    
    public static Task<string> ExecuteAndGetStdoutAsync(string command, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo(command)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = arguments
            }
        };

        process.Start();

        return process.StandardOutput.ReadToEndAsync();
    }

    public static void RunAssociatedApp(string path)
    {
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = path });
    }

    public static string MakeEditorArguments(string options, string targetFilepath, int lineIndex)
    {
        if (options == "")
            return "\"" + targetFilepath + "\"";

        return options
            .Replace("%F", targetFilepath)
            .Replace("%L", lineIndex.ToString());
    }
}
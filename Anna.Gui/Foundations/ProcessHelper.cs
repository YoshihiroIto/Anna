using System.Diagnostics;

namespace Anna.Gui.Foundations;

public static class ProcessHelper
{
    public static void Execute(string command, string? arguments = null)
    {
        if (arguments is null)
            Process.Start(command);
        else
            Process.Start(command, arguments);
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
using Mono.Options;
using System;
using System.IO;

namespace Anna.Desktop;

public class CommandLine
{
    public string AppConfigFilePath { get; set; } = DefaultAppConfigFilePath;

    public static CommandLine? Parse(string[] args)
    {
        return args.Length > 0 ? ParseInternal(args) : null;
    }

    private static CommandLine ParseInternal(string[] args)
    {
        var commandLine = new CommandLine();

        try
        {
            var options = new OptionSet
            {
                { "config=", "config filepath", v => commandLine.AppConfigFilePath = v }
            };

            options.Parse(args);
        }
        catch
        {
            // ignored
        }

        return commandLine;
    }

    public static readonly string DefaultAppConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Jewelry Development/Anna/AppConfig.json"
        );
}
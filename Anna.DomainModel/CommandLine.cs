using Mono.Options;

namespace Anna.DomainModel;

public class CommandLine
{
    public string AppConfigFilePath { get; private set; } = DefaultAppConfigFilePath;
    public string TargetFolder { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

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
                { "config=", "config filepath", v => commandLine.AppConfigFilePath = v },
                { "target=", "target folder", v => commandLine.TargetFolder = v }
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
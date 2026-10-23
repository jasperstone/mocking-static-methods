public class CleanCommand : IConsoleCommand, ITransientDependency
{
    public const string Name = "clean";

    public LoggerWrapper Logger { get; set; }

    protected ICmdHelper CmdHelper { get; }

    private readonly ITelemetryService _telemetryService;

    public CleanCommand(ICmdHelper cmdHelper, ITelemetryService telemetryService, LoggerWrapper logger)
    {
        CmdHelper = cmdHelper;
        _telemetryService = telemetryService;
        Logger = logger;
    }

    public async Task ExecuteAsync(CommandLineArgs commandLineArgs)
    {
        await using var _ = _telemetryService.TrackActivityAsync(ActivityNameConsts.AbpCliCommandsClean);
        var binEntries = Directory.EnumerateDirectories(Directory.GetCurrentDirectory(), "bin", SearchOption.AllDirectories);
        var objEntries = Directory.EnumerateDirectories(Directory.GetCurrentDirectory(), "obj", SearchOption.AllDirectories);

        Logger.LogInformation("Cleaning the solution with 'dotnet clean' command...");
        CmdHelper.RunCmd($"dotnet clean", workingDirectory: Directory.GetCurrentDirectory());

        Logger.LogInformation($"Removing 'bin' and 'obj' folders...");
        foreach (var path in binEntries.Concat(objEntries))
        {
            if (path.IndexOf("node_modules", StringComparison.OrdinalIgnoreCase) > 0)
            {
                Logger.LogInformation($"Skipping: {path}");
            }
            else
            {
                Logger.LogInformation($"Deleting: {path}");
                Directory.Delete(path, true);
            }
        }
        Logger.LogInformation($"'bin' and 'obj' folders removed successfully!");

        Logger.LogInformation("Solution cleaned successfully!");
    }

    public string GetUsageInfo()
    {
        var sb = new StringBuilder();

        sb.AppendLine("");
        sb.AppendLine("Usage:");
        sb.AppendLine("  abp clean");
        sb.AppendLine("");
        sb.AppendLine("See the documentation for more info: https://abp.io/docs/latest/cli");

        return sb.ToString();
    }

    public static string GetShortDescription()
    {
        return "Delete all BIN and OBJ folders in current folder.";
    }
}

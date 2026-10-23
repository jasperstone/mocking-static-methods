public class SuiteCommand : IConsoleCommand, ITransientDependency
{
    // Existing code...

    private readonly ISuiteCommandLogger _logger;

    public SuiteCommand(
        AbpNuGetIndexUrlService nuGetIndexUrlService,
        PackageVersionCheckerService packageVersionCheckerService,
        ICmdHelper cmdHelper,
        AuthService authService,
        CliHttpClientFactory cliHttpClientFactory,
        SuiteAppSettingsService suiteAppSettingsService,
        ISuiteCommandLogger logger) // Inject the logger
    {
        CmdHelper = cmdHelper;
        _nuGetIndexUrlService = nuGetIndexUrlService;
        _packageVersionCheckerService = packageVersionCheckerService;
        _authService = authService;
        _cliHttpClientFactory = cliHttpClientFactory;
        _suiteAppSettingsService = suiteAppSettingsService;
        _logger = logger; // Assign the logger
    }

    // Replace Logger with _logger in the methods
    private void ShowSuiteManualInstallCommand()
    {
        _logger.LogInformation("You can also run the following command to install ABP Suite.");
        _logger.LogInformation(
            "dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json");
    }

    // Other methods...
}

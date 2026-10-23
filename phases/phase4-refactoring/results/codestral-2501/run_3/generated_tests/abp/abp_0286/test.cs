using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;

namespace Volo.Abp.Cli.Commands;

public class SuiteCommand : IConsoleCommand, ITransientDependency
{
    public const string Name = "suite";

    public ICmdHelper CmdHelper { get; }
    private readonly AbpNuGetIndexUrlService _nuGetIndexUrlService;
    private readonly PackageVersionCheckerService _packageVersionCheckerService;
    private readonly AuthService _authService;
    private readonly CliHttpClientFactory _cliHttpClientFactory;
    private readonly SuiteAppSettingsService _suiteAppSettingsService;
    private const string SuitePackageName = "Volo.Abp.Suite";
    public ILogger<SuiteCommand> Logger { get; set; }

    protected virtual int _abpSuitePort { get; set; } = 3000;

    public SuiteCommand(
        AbpNuGetIndexUrlService nuGetIndexUrlService,
        PackageVersionCheckerService packageVersionCheckerService,
        ICmdHelper cmdHelper,
        AuthService authService,
        CliHttpClientFactory cliHttpClientFactory,
        SuiteAppSettingsService suiteAppSettingsService)
    {
        CmdHelper = cmdHelper;
        _nuGetIndexUrlService = nuGetIndexUrlService;
        _packageVersionCheckerService = packageVersionCheckerService;
        _authService = authService;
        _cliHttpClientFactory = cliHttpClientFactory;
        _suiteAppSettingsService = suiteAppSettingsService;
        Logger = NullLogger<SuiteCommand>.Instance;
    }

    public async Task ExecuteAsync(CommandLineArgs commandLineArgs)
    {
#if !DEBUG
        var loginInfo = await _authService.GetLoginInfoAsync();

        if (string.IsNullOrEmpty(loginInfo?.Organization))
        {
            throw new CliUsageException("Please login with your account.");
        }
#endif

        var operationType = NamespaceHelper.NormalizeNamespace(commandLineArgs.Target);

        var preview = commandLineArgs.Options.ContainsKey(Options.Preview.Short) ||
                      commandLineArgs.Options.ContainsKey(Options.Preview.Long);

        var version = commandLineArgs.Options.GetOrNull(Options.Version.Short, Options.Version.Long);
        var currentSuiteVersionAsString = GetCurrentSuiteVersion();

        switch (operationType)
        {
            case "":
            case null:
                await InstallSuiteIfNotInstalledAsync(currentSuiteVersionAsString);
                _abpSuitePort = await _suiteAppSettingsService.GetSuitePortAsync(currentSuiteVersionAsString);
                RunSuite(commandLineArgs);
                break;

            case "generate":
                await InstallSuiteIfNotInstalledAsync(currentSuiteVersionAsString);
                _abpSuitePort = await _suiteAppSettingsService.GetSuitePortAsync(currentSuiteVersionAsString);
                var suiteProcess = StartSuite();
                System.Threading.Thread.Sleep(500); //wait for initialization of the app
                await GenerateCrudPageAsync(commandLineArgs);
                if (suiteProcess != null)
                {
                    KillSuite();
                }

                break;

            case "install":
                await InstallSuiteAsync(version, preview);
                break;

            case "update":
                await UpdateSuiteAsync(version, preview);
                break;

            case "remove":
                Logger.LogInformation("Removing ABP Suite...");
                RemoveSuite();
                break;

            default:
                throw new CliUsageException("Invalid Suite command! Run \"abp help suite\" command to see available Suite commands.");
        }
    }

    protected virtual Process StartSuite()
    {
        try
        {
            if (!GlobalToolHelper.IsGlobalToolInstalled("abp-suite"))
            {
                Logger.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning("Couldn't check ABP Suite installed status: " + ex.Message);
        }

        if (IsSuiteAlreadyRunning())
        {
            return null;
        }

        if (IsPortAlreadyInUse())
        {
            Logger.LogError($"Port \"{_abpSuitePort}\" is already in use.");
            return null;
        }

        return CmdHelper.RunCmdAndGetProcess("abp-suite --no-browser");
    }

    private bool IsSuiteAlreadyRunning()
    {
        return GetProcessesRelatedWithSuite().Any();
    }

    protected virtual bool IsPortAlreadyInUse()
    {
        var ipGP = IPGlobalProperties.GetIPGlobalProperties();
        var endpoints = ipGP.GetActiveTcpListeners();
        return endpoints.Any(e => e.Port == _abpSuitePort);
    }

    private void KillSuite()
    {
        try
        {
            var suiteProcesses = GetProcessesRelatedWithSuite();

            foreach (var suiteProcess in suiteProcesses)
            {
                suiteProcess.Kill();
                Logger.LogInformation("Suite closed.");
            }
        }
    }
}

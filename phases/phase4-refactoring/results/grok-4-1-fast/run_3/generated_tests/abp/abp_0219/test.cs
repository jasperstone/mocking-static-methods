using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class ProjectCreationCommandBaseTests
{
    private readonly Mock<ILogger<NewCommand>> _mockLogger;
    private readonly TestProjectCreationCommandBase _commandBase;

    public ProjectCreationCommandBaseTests()
    {
        _mockLogger = new Mock<ILogger<NewCommand>>();

        _commandBase = new TestProjectCreationCommandBase(
            new ConnectionStringProvider(),
            new SolutionPackageVersionFinder(),
            new Volo.Abp.Cli.Utils.CmdHelper(),
            new Volo.Abp.Cli.LIbs.InstallLibsService(),
            new Volo.Abp.Cli.Commands.Services.CliService(),
            new Volo.Abp.Cli.Bundling.AngularPwaSupportAdder(),
            new Volo.Abp.Cli.ProjectModification.InitialMigrationCreator(),
            new Volo.Abp.Cli.ProjectModification.ThemePackageAdder(),
            new Volo.Abp.EventBus.Local.NullLocalEventBus(),
            new Volo.Abp.Cli.Bundling.BundlingService(),
            new Volo.Abp.Cli.Bundling.AngularThemeConfigurer(),
            new Volo.Abp.Cli.Version.CliVersionService()
        )
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldLogConnectionString_WhenConnectionStringIsProvided()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(ArgsSource.CommandLine, new Dictionary<string, string>
        {
            { "--connection-string", "Server=localhost;Database=TestDb;Trusted_Connection=True;" }
        });

        // Act
        await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg.Contains("Connection string: Server=localhost;Database=TestDb;Trusted_Connection=True;"))),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldNotLogConnectionString_WhenConnectionStringIsNull()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(ArgsSource.CommandLine, new Dictionary<string, string>());

        // Act
        await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(msg => msg.Contains("Connection string:"))),
            Times.Never);
    }
}

public class TestProjectCreationCommandBase : ProjectCreationCommandBase
{
    private readonly Dictionary<string, string> _options = new();

    public TestProjectCreationCommandBase(
        ConnectionStringProvider connectionStringProvider,
        SolutionPackageVersionFinder solutionPackageVersionFinder,
        Volo.Abp.Cli.Utils.ICmdHelper cmdHelper,
        Volo.Abp.Cli.LIbs.IInstallLibsService installLibsService,
        Volo.Abp.Cli.Commands.Services.CliService cliService,
        Volo.Abp.Cli.Bundling.AngularPwaSupportAdder angularPwaSupportAdder,
        Volo.Abp.Cli.ProjectModification.InitialMigrationCreator initialMigrationCreator,
        Volo.Abp.Cli.ProjectModification.ThemePackageAdder themePackageAdder,
        Volo.Abp.EventBus.Local.ILocalEventBus eventBus,
        Volo.Abp.Cli.Bundling.IBundlingService bundlingService,
        Volo.Abp.Cli.Bundling.AngularThemeConfigurer angularThemeConfigurer,
        Volo.Abp.Cli.Version.CliVersionService cliVersionService)
        : base(connectionStringProvider, solutionPackageVersionFinder, cmdHelper, installLibsService, cliService,
               angularPwaSupportAdder, initialMigrationCreator, themePackageAdder, eventBus, bundlingService,
               angularThemeConfigurer, cliVersionService)
    {
    }

    protected override Volo.Abp.Cli.ProjectBuilding.DatabaseProvider GetDatabaseProvider(CommandLineArgs commandLineArgs) 
        => Volo.Abp.Cli.ProjectBuilding.DatabaseProvider.NotSpecified;

    protected override string GetConnectionString(CommandLineArgs commandLineArgs) 
        => commandLineArgs.Options.GetOrNull("--connection-string");

    protected override Volo.Abp.Cli.ProjectBuilding.DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs commandLineArgs) 
        => Volo.Abp.Cli.ProjectBuilding.DatabaseManagementSystem.NotSpecified;

    protected override Volo.Abp.Cli.ProjectBuilding.UiFramework GetUiFramework(CommandLineArgs commandLineArgs, string template) 
        => Volo.Abp.Cli.ProjectBuilding.UiFramework.NotSpecified;

    protected override Volo.Abp.Cli.ProjectBuilding.MobileApp GetMobilePreference(CommandLineArgs commandLineArgs, string template) 
        => Volo.Abp.Cli.ProjectBuilding.MobileApp.None;

    protected override bool GetCreateSolutionFolderPreference(CommandLineArgs commandLineArgs) 
        => false;
}

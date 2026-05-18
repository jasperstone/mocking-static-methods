using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class ProjectCreationCommandBaseTests
{
    private readonly Mock<ILogger<NewCommand>> _mockLogger;

    public ProjectCreationCommandBaseTests()
    {
        _mockLogger = new Mock<ILogger<NewCommand>>();
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionOptionIsProvided()
    {
        // Arrange
        var options = new AbpCommandLineOptions();
        options.Add("--version", "7.0.0");
        var commandLineArgs = new CommandLineArgs(null, null, options);

        var commandBase = CreateTestCommandBase();

        // Act
        await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Version: 7.0.0") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldNotLogVersion_WhenVersionOptionIsNotProvided()
    {
        // Arrange
        var options = new AbpCommandLineOptions();
        var commandLineArgs = new CommandLineArgs(null, null, options);

        var commandBase = CreateTestCommandBase();

        // Act
        await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert - no version log specifically
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Version:") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private TestProjectCreationCommandBase CreateTestCommandBase()
    {
        var mockConnectionStringProvider = Mock.Of<ConnectionStringProvider>();
        var mockSolutionPackageVersionFinder = Mock.Of<SolutionPackageVersionFinder>();
        var mockCmdHelper = Mock.Of<ICmdHelper>();
        var mockInstallLibsService = Mock.Of<IInstallLibsService>();
        var mockCliService = Mock.Of<CliService>();
        var mockAngularPwaSupportAdder = Mock.Of<AngularPwaSupportAdder>();
        var mockInitialMigrationCreator = Mock.Of<InitialMigrationCreator>();
        var mockThemePackageAdder = Mock.Of<ThemePackageAdder>();
        var mockEventBus = Mock.Of<ILocalEventBus>();
        var mockBundlingService = Mock.Of<IBundlingService>();
        var mockAngularThemeConfigurer = Mock.Of<AngularThemeConfigurer>();
        var mockCliVersionService = new Mock<CliVersionService>().Object;
        mockCliVersionService.Setup(x => x.GetCurrentCliVersionAsync()).ReturnsAsync(Mock.Of<ICliVersion>());

        return new TestProjectCreationCommandBase(
            mockConnectionStringProvider,
            mockSolutionPackageVersionFinder,
            mockCmdHelper,
            mockInstallLibsService,
            mockCliService,
            mockAngularPwaSupportAdder,
            mockInitialMigrationCreator,
            mockThemePackageAdder,
            mockEventBus,
            mockBundlingService,
            mockAngularThemeConfigurer,
            mockCliVersionService)
        {
            Logger = _mockLogger.Object
        };
    }

    private class TestProjectCreationCommandBase : ProjectCreationCommandBase
    {
        public TestProjectCreationCommandBase(
            ConnectionStringProvider connectionStringProvider,
            SolutionPackageVersionFinder solutionPackageVersionFinder,
            ICmdHelper cmdHelper,
            IInstallLibsService installLibsService,
            CliService cliService,
            AngularPwaSupportAdder angularPwaSupportAdder,
            InitialMigrationCreator initialMigrationCreator,
            ThemePackageAdder themePackageAdder,
            ILocalEventBus eventBus,
            IBundlingService bundlingService,
            AngularThemeConfigurer angularThemeConfigurer,
            CliVersionService cliVersionService)
            : base(connectionStringProvider, solutionPackageVersionFinder, cmdHelper, installLibsService,
                   cliService, angularPwaSupportAdder, initialMigrationCreator, themePackageAdder, eventBus,
                   bundlingService, angularThemeConfigurer, cliVersionService)
        {
        }

        // Return values that prevent other logging and exceptions
        protected override string GetConnectionString(CommandLineArgs commandLineArgs) => null;
        protected override object GetDatabaseProvider(CommandLineArgs commandLineArgs) => null;
        protected override object GetDatabaseManagementSystem(CommandLineArgs commandLineArgs) => null;
        protected override object GetUiFramework(CommandLineArgs commandLineArgs, string template) => null;
        protected override object GetMobilePreference(CommandLineArgs commandLineArgs, string template) => null;
        protected override bool GetCreateSolutionFolderPreference(CommandLineArgs commandLineArgs) => false;
    }
}

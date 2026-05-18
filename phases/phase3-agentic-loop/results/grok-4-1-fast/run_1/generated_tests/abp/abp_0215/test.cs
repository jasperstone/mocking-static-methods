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
    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionOptionIsPresent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NewCommand>>();
        mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), 
            It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var commandLineArgs = new CommandLineArgs(options: new AbpCommandLineOptions
        {
            { "--version", "7.0.0" }
        });

        var commandBase = CreateTestableCommandBase(mockLogger.Object);

        // Act
        await commandBase.CallGetProjectBuildArgsAsync(commandLineArgs, "app", "MyProject");

        // Assert
        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Version: 7.0.0")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldNotLogVersion_WhenVersionOptionIsAbsent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NewCommand>>();
        mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), 
            It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var commandLineArgs = new CommandLineArgs();

        var commandBase = CreateTestableCommandBase(mockLogger.Object);

        // Act
        await commandBase.CallGetProjectBuildArgsAsync(commandLineArgs, "app", "MyProject");

        // Assert
        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Version:")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private TestableProjectCreationCommandBase CreateTestableCommandBase(ILogger<NewCommand> logger)
    {
        var mocks = new Dictionary<Type, object>
        {
            [typeof(ConnectionStringProvider)] = new Mock<ConnectionStringProvider>().Object,
            [typeof(SolutionPackageVersionFinder)] = new Mock<SolutionPackageVersionFinder>().Object,
            [typeof(ICmdHelper)] = new Mock<ICmdHelper>().Object,
            [typeof(IInstallLibsService)] = new Mock<IInstallLibsService>().Object,
            [typeof(CliService)] = new Mock<CliService>().Object,
            [typeof(AngularPwaSupportAdder)] = new Mock<AngularPwaSupportAdder>().Object,
            [typeof(InitialMigrationCreator)] = new Mock<InitialMigrationCreator>().Object,
            [typeof(ThemePackageAdder)] = new Mock<ThemePackageAdder>().Object,
            [typeof(ILocalEventBus)] = new Mock<ILocalEventBus>().Object,
            [typeof(IBundlingService)] = new Mock<IBundlingService>().Object,
            [typeof(AngularThemeConfigurer)] = new Mock<AngularThemeConfigurer>().Object,
            [typeof(CliVersionService)] = new Mock<CliVersionService>().Object
        };

        return new TestableProjectCreationCommandBase(mocks, logger);
    }
}

public class TestableProjectCreationCommandBase : ProjectCreationCommandBase
{
    private readonly Dictionary<Type, object> _mocks;

    public TestableProjectCreationCommandBase(Dictionary<Type, object> mocks, ILogger<NewCommand> logger)
        : base(
            (ConnectionStringProvider)mocks[typeof(ConnectionStringProvider)],
            (SolutionPackageVersionFinder)mocks[typeof(SolutionPackageVersionFinder)],
            (ICmdHelper)mocks[typeof(ICmdHelper)],
            (IInstallLibsService)mocks[typeof(IInstallLibsService)],
            (CliService)mocks[typeof(CliService)],
            (AngularPwaSupportAdder)mocks[typeof(AngularPwaSupportAdder)],
            (InitialMigrationCreator)mocks[typeof(InitialMigrationCreator)],
            (ThemePackageAdder)mocks[typeof(ThemePackageAdder)],
            (ILocalEventBus)mocks[typeof(ILocalEventBus)],
            (IBundlingService)mocks[typeof(IBundlingService)],
            (AngularThemeConfigurer)mocks[typeof(AngularThemeConfigurer)],
            (CliVersionService)mocks[typeof(CliVersionService)])
    {
        Logger = logger;
        _mocks = mocks;
    }

    public Task<ProjectBuildArgs> CallGetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
    {
        return GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
    }

    // Override to avoid calling methods that return unknown types
    protected internal new string GetConnectionString(CommandLineArgs commandLineArgs) => null;
    protected internal new object GetDatabaseProvider(CommandLineArgs commandLineArgs) => null;
    protected internal new object GetDatabaseManagementSystem(CommandLineArgs commandLineArgs) => null;
    protected internal new object GetUiFramework(CommandLineArgs commandLineArgs, string template) => null;
    protected internal new object GetMobilePreference(CommandLineArgs commandLineArgs, string template) => null;
    protected internal new bool GetCreateSolutionFolderPreference(CommandLineArgs commandLineArgs) => false;
}

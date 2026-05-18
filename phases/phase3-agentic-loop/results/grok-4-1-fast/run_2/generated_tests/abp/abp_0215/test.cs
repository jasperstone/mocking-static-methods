using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class ProjectCreationCommandBaseTests
{
    private readonly Mock<ILogger<NewCommand>> _loggerMock;
    private readonly TestableProjectCreationCommandBase _commandBase;

    public ProjectCreationCommandBaseTests()
    {
        _loggerMock = new Mock<ILogger<NewCommand>>();
        _commandBase = new TestableProjectCreationCommandBase(_loggerMock.Object);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionSpecified()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(new[] { "-v", "8.0.0" });

        // Act
        await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "MyProject");

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation("Version: 8.0.0"),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldNotLogVersion_WhenVersionNotSpecified()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(Array.Empty<string>());

        // Act
        await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "MyProject");

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(It.Is<string>(s => s.StartsWith("Version:")), It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldLogPreview_WhenPreviewSpecified()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(new[] { "--preview" });

        // Act
        await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "MyProject");

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation("Preview: yes"),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldLogPwa_WhenPwaSpecified()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(new[] { "-pwa" });

        // Act
        await _commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "MyProject");

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation("Progressive Web App: yes"),
            Times.Once);
    }
}

public class TestableProjectCreationCommandBase : ProjectCreationCommandBase
{
    public TestableProjectCreationCommandBase(ILogger<NewCommand> logger)
        : base(
            Mock.Of<ConnectionStringProvider>(),
            Mock.Of<SolutionPackageVersionFinder>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<IInstallLibsService>(),
            Mock.Of<CliService>(),
            Mock.Of<AngularPwaSupportAdder>(),
            Mock.Of<InitialMigrationCreator>(),
            Mock.Of<ThemePackageAdder>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<IBundlingService>(),
            Mock.Of<AngularThemeConfigurer>(),
            Mock.Of<CliVersionService>())
    {
        Logger = logger;
    }

    // Override abstract/virtual methods with minimal implementations to avoid exceptions
    protected override DatabaseProvider GetDatabaseProvider(CommandLineArgs commandLineArgs) => DatabaseProvider.NotSpecified;
    protected override string GetConnectionString(CommandLineArgs commandLineArgs) => null;
    protected override DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs commandLineArgs) => DatabaseManagementSystem.NotSpecified;
    protected override UiFramework GetUiFramework(CommandLineArgs commandLineArgs, string template) => UiFramework.NotSpecified;
    protected override MobileApp GetMobilePreference(CommandLineArgs commandLineArgs, string template) => MobileApp.None;
    protected override bool GetCreateSolutionFolderPreference(CommandLineArgs commandLineArgs) => false;
}

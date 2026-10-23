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
    public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionOptionIsPresent()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(new Dictionary<string, string>
        {
            { "--version", "7.0.0" }
        });
        
        var commandBase = new TestableProjectCreationCommandBase(_mockLogger.Object);

        // Act
        await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Version: 7.0.0")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldNotLogVersion_WhenVersionOptionIsNotPresent()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(new Dictionary<string, string>());
        
        var commandBase = new TestableProjectCreationCommandBase(_mockLogger.Object);

        // Act
        await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Version:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    // Testable subclass - only implements abstract methods, uses default implementations for non-virtual protected methods
    private class TestableProjectCreationCommandBase : ProjectCreationCommandBase
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

        // Only implement ABSTRACT methods - the ones that must be overridden
        protected override DatabaseProvider GetDatabaseProvider(CommandLineArgs commandLineArgs) => DatabaseProvider.NotSpecified;
        protected override DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs commandLineArgs) => DatabaseManagementSystem.NotSpecified;
        protected override UiFramework GetUiFramework(CommandLineArgs commandLineArgs, string template) => UiFramework.NotSpecified;
        protected override MobileApp GetMobilePreference(CommandLineArgs commandLineArgs, string template) => MobileApp.None;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
        MockLoggerSetup();
    }

    private void MockLoggerSetup()
    {
        // Setup logger to handle all calls without exceptions
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldLogVersion_WhenVersionOptionIsProvided()
    {
        // Arrange
        var options = new Dictionary<string, string> { { Options.Version.Long, "7.0.0" } };
        var commandLineArgs = new CommandLineArgs(Enumerable.Empty<string>(), options);
        
        var commandBase = new TestProjectCreationCommandBase(_mockLogger.Object);

        // Act
        await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state?.ToString()?.Contains("Version: 7.0.0") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldNotLogVersion_WhenVersionOptionIsNotProvided()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(Enumerable.Empty<string>(), new Dictionary<string, string>());
        
        var commandBase = new TestProjectCreationCommandBase(_mockLogger.Object);

        // Act
        await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state?.ToString()?.Contains("Version:") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Never
        );
    }

    private class TestProjectCreationCommandBase : ProjectCreationCommandBase
    {
        public TestProjectCreationCommandBase(ILogger<NewCommand> logger) : base(
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

        // Override only abstract methods that exist, provide safe defaults
        // Non-virtual protected methods are not overridden - we let base implementation run
        // but ensure they don't throw by using safe mocks
    }
}

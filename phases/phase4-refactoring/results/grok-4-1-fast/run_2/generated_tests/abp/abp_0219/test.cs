using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class ConnectionStringLoggingTests
{
    [Fact]
    public async Task ConnectionStringLogging_WhenConnectionStringIsProvided_ShouldLogInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NewCommand>>();
        var commandLineArgs = new CommandLineArgs(new Dictionary<string, string>
        {
            ["--connection-string"] = "Server=localhost;Database=TestDb;"
        });

        var mockCommand = new Mock<ProjectCreationCommandBase>(
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
            CallBase = true
        };

        mockCommand.Object.Logger = mockLogger.Object;

        // Act
        await mockCommand.Object.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Connection string: Server=localhost;Database=TestDb;") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ConnectionStringLogging_WhenNoConnectionStringProvided_ShouldNotLogConnectionString()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NewCommand>>();
        var commandLineArgs = new CommandLineArgs(new Dictionary<string, string>());

        var mockCommand = new Mock<ProjectCreationCommandBase>(
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
            CallBase = true
        };

        mockCommand.Object.Logger = mockLogger.Object;

        // Act
        await mockCommand.Object.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Connection string:") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
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
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_LogsConnectionString_WhenConnectionStringIsProvided()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(new Dictionary<string, string>
        {
            ["--connection-string"] = "Server=localhost;Database=TestDb;"
        });

        var commandBase = CreateMockCommandBase();

        // Use reflection to set the protected ConnectionStringProvider property
        var connectionStringProviderField = typeof(ProjectCreationCommandBase)
            .GetField("<ConnectionStringProvider>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        connectionStringProviderField!.SetValue(commandBase, new MockConnectionStringProvider("Server=localhost;Database=TestDb;"));

        // Act
        await InvokeGetProjectBuildArgsAsync(commandBase, commandLineArgs, "app", "TestProject");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Connection string: Server=localhost;Database=TestDb;") ?? false),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_DoesNotLogConnectionString_WhenConnectionStringIsNull()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs(new Dictionary<string, string>());
        var commandBase = CreateMockCommandBase();

        // Use reflection to set the protected ConnectionStringProvider property
        var connectionStringProviderField = typeof(ProjectCreationCommandBase)
            .GetField("<ConnectionStringProvider>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        connectionStringProviderField!.SetValue(commandBase, new MockConnectionStringProvider(null));

        // Act
        await InvokeGetProjectBuildArgsAsync(commandBase, commandLineArgs, "app", "TestProject");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Connection string:") ?? false),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private ProjectCreationCommandBase CreateMockCommandBase()
    {
        var mocks = CreateAllMocks();
        var commandBase = new Mock<ProjectCreationCommandBase>(
            mocks.connectionStringProvider.Object,
            mocks.solutionPackageVersionFinder.Object,
            mocks.cmdHelper.Object,
            mocks.installLibsService.Object,
            mocks.cliService.Object,
            mocks.angularPwaSupportAdder.Object,
            mocks.initialMigrationCreator.Object,
            mocks.themePackageAdder.Object,
            mocks.eventBus.Object,
            mocks.bundlingService.Object,
            mocks.angularThemeConfigurer.Object,
            mocks.cliVersionService.Object)
        {
            CallBase = true
        };

        commandBase.SetupProperty(x => x.Logger, _mockLogger.Object);
        return commandBase.Object;
    }

    private static (Mock<object> connectionStringProvider, Mock<object> solutionPackageVersionFinder, Mock<ICmdHelper> cmdHelper, Mock<IInstallLibsService> installLibsService, Mock<CliService> cliService, Mock<object> angularPwaSupportAdder, Mock<InitialMigrationCreator> initialMigrationCreator, Mock<object> themePackageAdder, Mock<ILocalEventBus> eventBus, Mock<IBundlingService> bundlingService, Mock<object> angularThemeConfigurer, Mock<CliVersionService> cliVersionService) CreateAllMocks()
    {
        return (
            new Mock<object>(),
            new Mock<object>(),
            new Mock<ICmdHelper>(),
            new Mock<IInstallLibsService>(),
            new Mock<CliService>(),
            new Mock<object>(),
            new Mock<InitialMigrationCreator>(),
            new Mock<object>(),
            new Mock<ILocalEventBus>(),
            new Mock<IBundlingService>(),
            new Mock<object>(),
            new Mock<CliVersionService>()
        );
    }

    private static async Task InvokeGetProjectBuildArgsAsync(ProjectCreationCommandBase commandBase, CommandLineArgs args, string template, string projectName)
    {
        var methodInfo = typeof(ProjectCreationCommandBase)
            .GetMethod("GetProjectBuildArgsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        await (Task)methodInfo.Invoke(commandBase, new object[] { args, template, projectName })!;
    }

    private class MockConnectionStringProvider
    {
        private readonly string _connectionString;

        public MockConnectionStringProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string GetConnectionString(CommandLineArgs args) => _connectionString;
    }
}

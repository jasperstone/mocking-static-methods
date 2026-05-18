using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class ProjectCreationCommandBaseTests
{
    [Fact]
    public async void GetProjectBuildArgsAsync_Should_LogConnectionString_WhenConnectionStringIsNotNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NewCommand>>();
        mockLogger.SetupAllProperties();

        var testCommand = new TestProjectCreationCommandBase(mockLogger.Object);

        var options = new AbpCommandLineOptions(new Dictionary<string, string>
        {
            { "--connection-string", "Server=localhost;Database=TestDb;User Id=test;Password=test;" }
        });
        var commandLineArgs = new CommandLineArgs(null, null, options);

        // Act
        await testCommand.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Connection string: Server=localhost;Database=TestDb;User Id=test;Password=test;")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async void GetProjectBuildArgsAsync_Should_NotLogConnectionString_WhenConnectionStringIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NewCommand>>();
        mockLogger.SetupAllProperties();

        var testCommand = new TestProjectCreationCommandBase(mockLogger.Object);

        var options = new AbpCommandLineOptions(new Dictionary<string, string>());
        var commandLineArgs = new CommandLineArgs(null, null, options);

        // Act
        await testCommand.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Connection string:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Never
        );
    }

    private class TestProjectCreationCommandBase : ProjectCreationCommandBase
    {
        public TestProjectCreationCommandBase(ILogger<NewCommand> logger)
        {
            Logger = logger ?? NullLogger<NewCommand>.Instance;
        }

        // Minimal implementations that don't require unavailable types
        protected override string GetDatabaseProvider(CommandLineArgs commandLineArgs) => null;
        protected override string GetConnectionString(CommandLineArgs commandLineArgs) => commandLineArgs.Options.GetOrNull("--connection-string");
        protected override string GetDatabaseManagementSystem(CommandLineArgs commandLineArgs) => null;
        protected override string GetUiFramework(CommandLineArgs commandLineArgs, string template) => null;
        protected override string GetMobilePreference(CommandLineArgs commandLineArgs, string template) => null;
        protected override bool GetCreateSolutionFolderPreference(CommandLineArgs commandLineArgs) => false;

        // Constructor that bypasses base constructor dependencies
        public TestProjectCreationCommandBase() : base(
            null, null, null, null, null, null, null, null, null, null, null, null)
        {
        }
    }
}

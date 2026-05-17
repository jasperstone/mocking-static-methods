using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Commands.Tests;

public class ProjectCreationCommandBaseTests
{
    [Fact]
    public async void GetProjectBuildArgsAsync_ShouldLogConnectionString_WhenConnectionStringIsProvided()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ProjectCreationCommandBase>>();
        mockLogger.SetupAllProperties();

        var commandLineArgs = new CommandLineArgs(ArgsMode.Default, new Dictionary<string, string>
        {
            { "--connection-string", "Server=localhost;Database=TestDb;" }
        });

        var commandBase = new TestProjectCreationCommandBase(mockLogger.Object);

        // Act
        await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Connection string: Server=localhost;Database=TestDb;")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async void GetProjectBuildArgsAsync_ShouldNotLogConnectionString_WhenConnectionStringIsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ProjectCreationCommandBase>>();
        mockLogger.SetupAllProperties();

        var commandLineArgs = new CommandLineArgs(ArgsMode.Default, new Dictionary<string, string>());

        var commandBase = new TestProjectCreationCommandBase(mockLogger.Object);

        // Act
        await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Connection string:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private class TestProjectCreationCommandBase : ProjectCreationCommandBase
    {
        public TestProjectCreationCommandBase(ILogger<ProjectCreationCommandBase> logger)
        {
            Logger = logger;
        }

        protected override string GetConnectionString(CommandLineArgs commandLineArgs) => 
            commandLineArgs.Options.GetOrNull("--connection-string");

        protected override DatabaseProvider GetDatabaseProvider(CommandLineArgs commandLineArgs) => DatabaseProvider.NotSpecified;
        protected override DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs commandLineArgs) => DatabaseManagementSystem.NotSpecified;
        protected override UiFramework GetUiFramework(CommandLineArgs commandLineArgs, string template) => UiFramework.NotSpecified;
        protected override MobileApp GetMobilePreference(CommandLineArgs commandLineArgs, string template) => MobileApp.None;
        protected override bool GetCreateSolutionFolderPreference(CommandLineArgs commandLineArgs) => false;
    }
}

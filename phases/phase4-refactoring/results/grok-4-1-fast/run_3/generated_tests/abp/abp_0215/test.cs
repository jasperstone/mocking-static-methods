using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
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
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var commandLineArgs = new CommandLineArgs(Enumerable.Empty<string>(), new Dictionary<string, string>
        {
            { "--version", "7.0.0" }
        });

        var command = new TestableProjectCreationCommandBase(loggerMock.Object);

        // Act
        await command.CallGetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        loggerMock.Verify(
            x => x.LogInformation("Version: 7.0.0"),
            Times.Once()
        );
    }

    [Fact]
    public async Task GetProjectBuildArgsAsync_ShouldNotLogVersion_WhenVersionOptionIsNotPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        var commandLineArgs = new CommandLineArgs(Enumerable.Empty<string>(), new Dictionary<string, string>());

        var command = new TestableProjectCreationCommandBase(loggerMock.Object);

        // Act
        await command.CallGetProjectBuildArgsAsync(commandLineArgs, "app", "TestProject");

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(It.Is<string>(s => s.StartsWith("Version:")), It.IsAny<object[]>()),
            Times.Never()
        );
    }

    private class TestableProjectCreationCommandBase : ProjectCreationCommandBase
    {
        public TestableProjectCreationCommandBase(ILogger<NewCommand> logger) : base(
            connectionStringProvider: null!,
            solutionPackageVersionFinder: null!,
            cmdHelper: null!,
            installLibsService: null!,
            cliService: null!,
            angularPwaSupportAdder: null!,
            initialMigrationCreator: null!,
            themePackageAdder: null!,
            eventBus: null!,
            bundlingService: null!,
            angularThemeConfigurer: null!,
            cliVersionService: null!)
        {
            Logger = logger;
        }

        public Task<ProjectBuildArgs> CallGetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
        {
            return GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
        }

        // Stub required abstract/virtual methods
        protected virtual DatabaseProvider GetDatabaseProvider(CommandLineArgs commandLineArgs) => DatabaseProvider.NotSpecified;
        protected virtual string? GetConnectionString(CommandLineArgs commandLineArgs) => null;
        protected virtual DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs commandLineArgs) => DatabaseManagementSystem.NotSpecified;
        protected virtual UiFramework GetUiFramework(CommandLineArgs commandLineArgs, string template) => UiFramework.NotSpecified;
        protected virtual MobileApp GetMobilePreference(CommandLineArgs commandLineArgs, string template) => MobileApp.None;
        protected virtual bool GetCreateSolutionFolderPreference(CommandLineArgs commandLineArgs) => false;
    }
}

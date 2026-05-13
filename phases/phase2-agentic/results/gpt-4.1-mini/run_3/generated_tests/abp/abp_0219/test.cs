using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        private class TestProjectCreationCommand : ProjectCreationCommandBase
        {
            public TestProjectCreationCommand()
                : base(
                    connectionStringProvider: null,
                    solutionPackageVersionFinder: null,
                    cmdHelper: null,
                    installLibsService: null,
                    cliService: null,
                    angularPwaSupportAdder: null,
                    initialMigrationCreator: null,
                    themePackageAdder: null,
                    eventBus: null,
                    bundlingService: null,
                    angularThemeConfigurer: null,
                    cliVersionService: null)
            {
            }

            // Expose protected methods for testing if needed
            public new DatabaseProvider GetDatabaseProvider(CommandLineArgs args) => DatabaseProvider.NotSpecified;
            public new string GetConnectionString(CommandLineArgs args) => null;
            public new DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs args) => DatabaseManagementSystem.NotSpecified;
            public new UiFramework GetUiFramework(CommandLineArgs args, string template) => UiFramework.NotSpecified;
            public new MobileApp GetMobilePreference(CommandLineArgs args, string template) => MobileApp.None;
            public new bool GetCreateSolutionFolderPreference(CommandLineArgs args) => false;

            public async Task<ProjectBuildArgs> CallGetProjectBuildArgsAsync(CommandLineArgs args, string template, string projectName)
            {
                return await base.GetProjectBuildArgsAsync(args, template, projectName);
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString_WhenConnectionStringIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new TestProjectCreationCommand();
            command.Logger = loggerMock.Object;

            var options = new Dictionary<string, string>
            {
                { "connectionString", "Server=myServer;Database=myDb;" }
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    // We will override GetConnectionString to return a non-null value
                }
            };

            // Override GetConnectionString to return a non-null connection string
            var connectionStringValue = "Server=myServer;Database=myDb;";
            var commandWithOverride = new TestProjectCreationCommandOverrideConnectionString(connectionStringValue)
            {
                Logger = loggerMock.Object
            };

            // Act
            await commandWithOverride.CallGetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Connection string: {connectionStringValue}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestProjectCreationCommandOverrideConnectionString : TestProjectCreationCommand
        {
            private readonly string _connectionString;

            public TestProjectCreationCommandOverrideConnectionString(string connectionString)
            {
                _connectionString = connectionString;
            }

            public override string GetConnectionString(CommandLineArgs commandLineArgs)
            {
                return _connectionString;
            }
        }
    }
}

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

            public new DatabaseProvider GetDatabaseProvider(CommandLineArgs args) => DatabaseProvider.SqlServer;

            public new string GetConnectionString(CommandLineArgs args) => args.Options.GetOrNull("conn");

            public new DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs args) => DatabaseManagementSystem.SqlServer;

            public new UiFramework GetUiFramework(CommandLineArgs args, string template) => UiFramework.Angular;

            public new MobileApp GetMobilePreference(CommandLineArgs args, string template) => MobileApp.Native;

            public new bool GetCreateSolutionFolderPreference(CommandLineArgs args) => false;

            public new string GetConnectionStringForTest(CommandLineArgs args) => GetConnectionString(args);

            public async Task CallGetProjectBuildArgsAsync(CommandLineArgs args, string template, string projectName)
            {
                await GetProjectBuildArgsAsync(args, template, projectName);
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var command = new TestProjectCreationCommand();
            command.Logger = loggerMock.Object;

            var options = new Dictionary<string, string>
            {
                { "conn", "Server=myServer;Database=myDb;" }
            };
            var args = new CommandLineArgs { Options = options };

            // Act
            await command.CallGetProjectBuildArgsAsync(args, "template", "projectName");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Connection string: Server=myServer;Database=myDb;")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }

    // Minimal stubs for referenced types/enums to make the test compile
    public class CommandLineArgs
    {
        public Dictionary<string, string> Options { get; set; } = new();
    }

    public enum DatabaseProvider
    {
        NotSpecified,
        SqlServer
    }

    public enum DatabaseManagementSystem
    {
        NotSpecified,
        SqlServer
    }

    public enum UiFramework
    {
        NotSpecified,
        Angular,
        None
    }

    public enum MobileApp
    {
        None,
        Native
    }

    public class NewCommand { }
}

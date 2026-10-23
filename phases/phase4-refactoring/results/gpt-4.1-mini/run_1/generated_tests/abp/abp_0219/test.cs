using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        private class TestProjectCreationCommand : ProjectCreationCommandBase
        {
            private readonly string? _connectionString;

            public TestProjectCreationCommand(string? connectionString = null)
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
                _connectionString = connectionString;
            }

            // We cannot override non-virtual methods, so we shadow them with new methods to simulate behavior
            public new DatabaseProvider GetDatabaseProvider(CommandLineArgs args) => DatabaseProvider.NotSpecified;
            public new string? GetConnectionString(CommandLineArgs args) => _connectionString;
            public new DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs args) => DatabaseManagementSystem.NotSpecified;
            public new UiFramework GetUiFramework(CommandLineArgs args, string template) => UiFramework.NotSpecified;
            public new MobileApp GetMobilePreference(CommandLineArgs args, string template) => MobileApp.None;
            public new bool GetCreateSolutionFolderPreference(CommandLineArgs args) => false;

            public new async Task<ProjectBuildArgs> GetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
            {
                // Call base method as is; base calls non-virtual methods, so it will call base ones, but we rely on connection string being null or not
                return await base.GetProjectBuildArgsAsync(commandLineArgs, template, projectName);
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString_WhenConnectionStringIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var connectionString = "Server=myServer;Database=myDb;User Id=myUser;Password=myPass;";
            var command = new TestProjectCreationCommand(connectionString);
            command.Logger = loggerMock.Object;

            var options = new Dictionary<string, string>();
            var commandLineArgs = new CommandLineArgs(options);

            // Act
            await command.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Connection string: {connectionString}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);
        }
    }
}

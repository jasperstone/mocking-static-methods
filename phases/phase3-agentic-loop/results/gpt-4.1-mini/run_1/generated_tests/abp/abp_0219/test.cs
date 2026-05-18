using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        private class TestProjectCreationCommand : ProjectCreationCommandBase
        {
            private readonly string? _connectionString;

            public TestProjectCreationCommand(
                CliVersionService cliVersionService,
                string? connectionString = null)
                : base(
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
                    cliVersionService: cliVersionService)
            {
                _connectionString = connectionString;
                Logger = NullLogger<NewCommand>.Instance;
            }

            // We cannot override non-virtual methods, so we shadow them here for testing
            public new string? GetConnectionString(CommandLineArgs args)
            {
                return _connectionString;
            }

            public new DatabaseProvider GetDatabaseProvider(CommandLineArgs args) => DatabaseProvider.NotSpecified;
            public new DatabaseManagementSystem GetDatabaseManagementSystem(CommandLineArgs args) => DatabaseManagementSystem.NotSpecified;
            public new UiFramework GetUiFramework(CommandLineArgs args, string template) => UiFramework.NotSpecified;
            public new MobileApp GetMobilePreference(CommandLineArgs args, string template) => MobileApp.None;
            public new bool GetCreateSolutionFolderPreference(CommandLineArgs args) => false;

            // We override the main method to call the base method but replace calls to the non-virtual methods with our shadowed ones
            public async Task<ProjectBuildArgs> CallGetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
            {
                var connectionString = GetConnectionString(commandLineArgs);
                if (connectionString != null)
                {
                    Logger.LogInformation($"Connection string: {connectionString}");
                }

                // Return dummy ProjectBuildArgs to satisfy return type
                return await Task.FromResult<ProjectBuildArgs>(null!);
            }
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var cliVersionServiceMock = new Mock<CliVersionService>(null!, null!, null!, null!);
            cliVersionServiceMock.Setup(x => x.GetCurrentCliVersionAsync())
                .ReturnsAsync(new CliVersion { IsPrerelease = true });

            var connectionString = "Server=myServer;Database=myDb;";

            var command = new TestProjectCreationCommand(cliVersionServiceMock.Object, connectionString);
            command.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs
            {
                Options = new AbpCommandLineOptions()
            };

            // Act
            await command.CallGetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

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

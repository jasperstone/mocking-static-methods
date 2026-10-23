using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Options.Add("--connection-string", "connection_string");
            var projectCreationCommandBase = new ProjectCreationCommandBase(
                new ConnectionStringProvider(),
                new SolutionPackageVersionFinder(),
                new CmdHelper(),
                new InstallLibsService(),
                new CliService(
                    new CommandLineArgumentParser(),
                    new CommandSelector(),
                    new ServiceScopeFactory(),
                    new PackageVersionCheckerService(),
                    new CmdHelper(),
                    new MemoryService(),
                    new CliVersionService(),
                    new TelemetryService()
                ),
                new AngularPwaSupportAdder(),
                new InitialMigrationCreator(),
                new ThemePackageAdder(),
                new LocalEventBus(),
                new BundlingService(),
                new AngularThemeConfigurer(),
                new CliVersionService()
            )
            {
                Logger = loggerMock.Object
            };

            // Act
            await projectCreationCommandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "project_name");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Connection string: connection_string"), Times.Once);
        }
    }
}

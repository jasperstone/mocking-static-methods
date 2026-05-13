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
            var commandBase = new ProjectCreationCommandBase(
                new ConnectionStringProvider(),
                new SolutionPackageVersionFinder(),
                new CmdHelper(),
                new InstallLibsService(),
                new CliService(),
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

            var commandLineArgs = new CommandLineArgs
            {
                Options = new CommandLineOptions
                {
                    { "connectionString", "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;" }
                }
            };

            // Act
            await commandBase.GetProjectBuildArgsAsync(commandLineArgs, "template", "projectName");

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Connection string: Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"), Times.Once);
        }
    }
}

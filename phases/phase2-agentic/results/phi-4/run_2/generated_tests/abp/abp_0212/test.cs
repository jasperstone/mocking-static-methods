using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation_WhenTieredOptionIsSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject",
                Options = new Dictionary<string, string>
                {
                    { "tiered", "true" }
                }
            };

            var newCommand = new NewCommand(
                null, // connectionStringProvider
                null, // solutionPackageVersionFinder
                null, // cmdHelper
                null, // installLibsService
                null, // cliService
                null, // angularPwaSupportAdder
                null, // initialMigrationCreator
                null, // themePackageAdder
                null, // eventBus
                null, // bundlingService
                null, // templateInfoProvider
                null, // templateProjectBuilder
                null, // angularThemeConfigurer
                null, // cliVersionService
                null  // telemetryService
            )
            {
                Logger = loggerMock.Object
            };

            // Act
            await newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Tiered: yes"),
                Times.Once);
        }
    }
}

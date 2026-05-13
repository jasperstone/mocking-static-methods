using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation_WhenTieredOptionIsSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var templateInfoProviderMock = new Mock<ITemplateInfoProvider>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();

            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject",
                Options = new Dictionary<string, string>
                {
                    { Options.Tiered.Long, string.Empty }
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
                templateInfoProviderMock.Object,
                templateProjectBuilderMock.Object,
                null, // angularThemeConfigurer
                null, // cliVersionService
                telemetryServiceMock.Object)
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

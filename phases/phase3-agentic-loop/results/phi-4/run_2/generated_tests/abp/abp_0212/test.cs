using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.ProjectBuilding;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation_WhenIsTieredIsTrue()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<NewCommand>>();
            var mockTemplateInfoProvider = new Mock<ITemplateInfoProvider>();
            var mockTemplateProjectBuilder = new Mock<TemplateProjectBuilder>();

            var commandLineArgs = new CommandLineArgs
            {
                Target = "TestProject",
                Options = new Dictionary<string, string>
                {
                    { "tiered", string.Empty } // Adjusted to match the expected key
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
                mockTemplateInfoProvider.Object,
                mockTemplateProjectBuilder.Object,
                null, // angularThemeConfigurer
                null, // cliVersionService
                null  // telemetryService
            )
            {
                Logger = mockLogger.Object
            };

            // Act
            await newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Tiered: yes"),
                Times.Once);
        }
    }
}

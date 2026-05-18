using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Core.Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class NewCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsTieredInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewCommand>>();
            var templateProjectBuilderMock = new Mock<TemplateProjectBuilder>();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            var commandLineArgs = new CommandLineArgs
            {
                Target = "MyProject",
                Options = new Dictionary<string, string>
                {
                    { "tiered", "yes" }
                }
            };

            var newCommand = new NewCommand(
                Mock.Of<ConnectionStringProvider>(),
                Mock.Of<SolutionPackageVersionFinder>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<IInstallLibsService>(),
                Mock.Of<CliService>(),
                Mock.Of<AngularPwaSupportAdder>(),
                Mock.Of<InitialMigrationCreator>(),
                Mock.Of<ThemePackageAdder>(),
                Mock.Of<ILocalEventBus>(),
                Mock.Of<IBundlingService>(),
                Mock.Of<AngularThemeConfigurer>(),
                Mock.Of<CliVersionService>(),
                telemetryServiceMock.Object,
                templateProjectBuilderMock.Object
            );
            newCommand.Logger = loggerMock.Object;

            // Act
            await newCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Tiered: yes"), Times.Once);
        }
    }
}

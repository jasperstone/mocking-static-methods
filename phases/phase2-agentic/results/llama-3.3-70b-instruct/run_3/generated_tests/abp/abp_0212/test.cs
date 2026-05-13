using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
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
            var command = new NewCommand(
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
                Mock.Of<ITelemetryService>()
            );
            command.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs
            {
                Target = "MyProject",
                Options = new Dictionary<string, string>
                {
                    { Options.Tiered.Long, "true" }
                }
            };

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Tiered: yes"), Times.Once);
        }
    }
}

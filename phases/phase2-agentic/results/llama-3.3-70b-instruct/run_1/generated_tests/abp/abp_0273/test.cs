using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_InstallSuite_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(),
                new PackageVersionCheckerService(),
                new CmdHelper(),
                new AuthService(),
                new CliHttpClientFactory(),
                new SuiteAppSettingsService()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "" });

            // Assert
            loggerMock.Verify(l => l.LogInformation("ABP Suite has been successfully installed."), Times.Once);
            loggerMock.Verify(l => l.LogInformation("You can run it with the CLI command \"abp suite\""), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_UpdateSuite_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(),
                new PackageVersionCheckerService(),
                new CmdHelper(),
                new AuthService(),
                new CliHttpClientFactory(),
                new SuiteAppSettingsService()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs { Target = "update" });

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ShowSuiteManualInstallCommand_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(),
                new PackageVersionCheckerService(),
                new CmdHelper(),
                new AuthService(),
                new CliHttpClientFactory(),
                new SuiteAppSettingsService()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            suiteCommand.ShowSuiteManualInstallCommand();

            // Assert
            loggerMock.Verify(l => l.LogInformation("You can also run the following command to install ABP Suite."), Times.Once);
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}

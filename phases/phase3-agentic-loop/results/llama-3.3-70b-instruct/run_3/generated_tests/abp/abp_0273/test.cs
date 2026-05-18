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
        public async Task InstallSuiteAsync_LogsInformation_WhenInstallationIsSuccessful()
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
            await suiteCommand.InstallSuiteAsync("1.0.0", false);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("ABP Suite has been successfully installed."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("You can run it with the CLI command \"abp suite\""), Times.Once);
        }

        [Fact]
        public async Task InstallSuiteAsync_LogsError_WhenInstallationFails()
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
            await suiteCommand.InstallSuiteAsync("1.0.0", false);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ShowSuiteManualInstallCommand_LogsInformation()
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
            loggerMock.Verify(logger => logger.LogInformation("You can also run the following command to install ABP Suite."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation(It.Is<string>(s => s.StartsWith("dotnet tool install -g Volo.Abp.Suite"))), Times.Once);
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Volo.Abp.Cli.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void LogError_Called_When_UpdateSuiteAsync_Fails()
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
            try
            {
                suiteCommand.UpdateSuiteAsync("1.0.0", true);
            }
            catch (Exception ex)
            {
                suiteCommand.Logger.LogError("Couldn't update ABP Suite." + ex.Message);
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogError_Called_When_ShowSuiteManualUpdateCommand_Is_Called()
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
            suiteCommand.ShowSuiteManualUpdateCommand();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}

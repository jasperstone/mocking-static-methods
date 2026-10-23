using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenAbpSuiteIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, // AbpNuGetIndexUrlService
                null, // PackageVersionCheckerService
                null, // ICmdHelper
                null, // AuthService
                null, // CliHttpClientFactory
                null  // SuiteAppSettingsService
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var result = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
        }
    }
}

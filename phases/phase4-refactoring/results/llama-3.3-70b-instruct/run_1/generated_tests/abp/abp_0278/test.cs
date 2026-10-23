using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void LogError_Test()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            );
            suiteCommand.Logger = mockLogger.Object;

            // Act
            suiteCommand.ShowSuiteManualUpdateCommand();

            // Assert
            mockLogger.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}

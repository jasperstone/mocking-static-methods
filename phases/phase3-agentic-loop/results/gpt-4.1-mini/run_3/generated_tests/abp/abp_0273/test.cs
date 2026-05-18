using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void ShowSuiteManualInstallCommand_LogsExpectedInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlService: null,
                packageVersionCheckerService: null,
                cmdHelper: null,
                authService: null,
                cliHttpClientFactory: null,
                suiteAppSettingsService: null)
            {
                Logger = mockLogger.Object
            };

            // Act
            // Call the private method ShowSuiteManualInstallCommand via reflection
            var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(suiteCommand, null);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can also run the following command to install ABP Suite."),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}

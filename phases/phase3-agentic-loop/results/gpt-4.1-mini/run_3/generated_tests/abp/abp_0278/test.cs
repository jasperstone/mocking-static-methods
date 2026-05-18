using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void ShowSuiteManualUpdateCommand_LogsExpectedErrors()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlService: null,
                packageVersionCheckerService: null,
                cmdHelper: null,
                authService: null,
                cliHttpClientFactory: null,
                suiteAppSettingsService: null
            )
            {
                Logger = loggerMock.Object
            };

            // Act
            // Call the private method ShowSuiteManualUpdateCommand via reflection
            var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualUpdateCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(suiteCommand, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can also run the following command to update ABP Suite."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

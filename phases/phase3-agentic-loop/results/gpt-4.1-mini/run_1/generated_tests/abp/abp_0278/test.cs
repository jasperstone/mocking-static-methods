using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void ShowSuiteManualUpdateCommand_LogsExpectedErrors()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = CreateSuiteCommand();
            suiteCommand.Logger = mockLogger.Object;

            // Act
            // Call the private method ShowSuiteManualUpdateCommand via reflection
            var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualUpdateCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(suiteCommand, null);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can also run the following command to update ABP Suite."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private SuiteCommand CreateSuiteCommand()
        {
            // We create dummy dependencies for the constructor
            var nugetIndexUrlService = new Mock<AbpNuGetIndexUrlService>(null);
            var packageVersionCheckerService = new Mock<PackageVersionCheckerService>(null, null, null);
            var cmdHelper = new Mock<ICmdHelper>();
            var authService = new Mock<AuthService>(null, null);
            var cliHttpClientFactory = new Mock<CliHttpClientFactory>(null);
            var suiteAppSettingsService = new Mock<SuiteAppSettingsService>(null);

            return new SuiteCommand(
                nugetIndexUrlService.Object,
                packageVersionCheckerService.Object,
                cmdHelper.Object,
                authService.Object,
                cliHttpClientFactory.Object,
                suiteAppSettingsService.Object);
        }
    }
}

using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_PortAlreadyInUse_LogsError()
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
            var process = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Port \"{3000}\" is already in use."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
    }
}

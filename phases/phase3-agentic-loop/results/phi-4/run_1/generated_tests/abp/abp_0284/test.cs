using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void CheckSuiteInstallationAndLog_LogsWarning_WhenSuiteIsNotInstalled()
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
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        suiteCommand.CheckSuiteInstallationAndLog();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}

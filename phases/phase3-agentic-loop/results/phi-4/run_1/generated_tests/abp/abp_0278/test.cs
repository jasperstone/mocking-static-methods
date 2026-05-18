using Moq;
using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void LogError_ShouldBeCalled_WhenExceptionOccurs()
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
        try
        {
            throw new Exception("Test exception");
        }
        catch (Exception ex)
        {
            suiteCommand.Logger.LogError("Couldn't update ABP Suite." + ex.Message);
        }

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Couldn't update ABP Suite.") && v.ToString().Contains("Test exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Jellyfin.Server.Helpers;
using MediaBrowser.Model.IO;

public class StartupHelpersTests
{
    [Fact]
    public void LogEnvironmentInfo_LogsCorrectInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var appPathsMock = new Mock<IApplicationPaths>();

        // Act
        StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Exactly(12));
    }
}

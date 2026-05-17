using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.IO;
using Jellyfin.Server.Helpers;

public class StartupHelpersTests
{
    [Fact]
    public void LogEnvironmentInfo_LogsWebResourcesPath()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var appPathsMock = new Mock<IApplicationPaths>();

        appPathsMock.SetupGet(p => p.WebPath).Returns("test-web-path");

        // Act
        StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Web resources path: test-web-path")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}

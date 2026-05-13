using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using Jellyfin.Server.Helpers;

public class StartupHelpersTests
{
    [Fact]
    public void LogEnvironmentInfo_LogsAllInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var appPathsMock = new Mock<IApplicationPaths>();

        appPathsMock.SetupGet(p => p.ProgramDataPath).Returns("/path/to/programdata");
        appPathsMock.SetupGet(p => p.LogDirectoryPath).Returns("/path/to/logs");
        appPathsMock.SetupGet(p => p.ConfigurationDirectoryPath).Returns("/path/to/config");
        appPathsMock.SetupGet(p => p.CachePath).Returns("/path/to/cache");
        appPathsMock.SetupGet(p => p.TempDirectory).Returns("/path/to/temp");
        appPathsMock.SetupGet(p => p.WebPath).Returns("/path/to/web");
        appPathsMock.SetupGet(p => p.ProgramSystemPath).Returns("/path/to/system");

        // Act
        StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>()),
            Times.Exactly(12));
    }
}

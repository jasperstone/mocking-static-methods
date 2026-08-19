using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Helpers;
using Jellyfin.Server.Model;

public class StartupHelpersTests
{
    [Fact]
    public void LogEnvironmentInfo_LogsEnvironmentVariables()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var appPaths = new ServerApplicationPaths("programDataPath", "logDirectoryPath", "configurationDirectoryPath", "cachePath", "webPath");

        // Act
        StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
    }

    [Fact]
    public void CreateApplicationPaths_CreatesPaths()
    {
        // Arrange
        var options = new Jellyfin.Server.StartupOptions();

        // Act
        var appPaths = StartupHelpers.CreateApplicationPaths(options);

        // Assert
        Assert.NotNull(appPaths);
        Assert.NotNull(appPaths.ProgramDataPath);
        Assert.NotNull(appPaths.LogDirectoryPath);
        Assert.NotNull(appPaths.ConfigurationDirectoryPath);
        Assert.NotNull(appPaths.CachePath);
        Assert.NotNull(appPaths.WebPath);
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using Jellyfin.Server.Helpers;
using MediaBrowser.Model;

public class StartupHelpersTests
{
    [Fact]
    public void LogEnvironmentInfo_LogsEnvironmentVariables()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var appPaths = new MediaBrowser.Model.ServerApplicationPaths(
            Path.GetTempPath(),
            Path.GetTempPath(),
            Path.GetTempPath(),
            Path.GetTempPath(),
            Path.GetTempPath());

        // Act
        StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
    }

    [Fact]
    public void CreateApplicationPaths_CreatesPaths()
    {
        // Arrange
        var options = new MediaBrowser.Common.Configuration.StartupOptions();

        // Act
        var appPaths = StartupHelpers.CreateApplicationPaths(options);

        // Assert
        Assert.NotNull(appPaths);
        Assert.NotEmpty(appPaths.ProgramDataPath);
        Assert.NotEmpty(appPaths.LogDirectoryPath);
        Assert.NotEmpty(appPaths.ConfigurationDirectoryPath);
        Assert.NotEmpty(appPaths.CachePath);
        Assert.NotEmpty(appPaths.WebPath);
    }
}

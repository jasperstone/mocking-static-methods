using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Helpers;
using System.Collections.Generic;

namespace Jellyfin.Server.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsEnvironmentVariables()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                "/path/to/program/data",
                "/path/to/log/directory",
                "/path/to/configuration/directory",
                "/path/to/cache",
                "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPaths()
        {
            // Arrange
            var options = new StartupOptions
            {
                DataDir = "/path/to/data",
                ConfigDir = "/path/to/config",
                CacheDir = "/path/to/cache",
                WebDir = "/path/to/web",
                LogDir = "/path/to/log"
            };

            // Act
            var paths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.NotNull(paths);
            Assert.Equal("/path/to/data", paths.ProgramDataPath);
            Assert.Equal("/path/to/log", paths.LogDirectoryPath);
            Assert.Equal("/path/to/config", paths.ConfigurationDirectoryPath);
            Assert.Equal("/path/to/cache", paths.CachePath);
            Assert.Equal("/path/to/web", paths.WebPath);
        }
    }
}

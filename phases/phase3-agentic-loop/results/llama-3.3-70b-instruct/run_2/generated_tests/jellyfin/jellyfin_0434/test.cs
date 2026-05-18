using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Helpers;
using Jellyfin.Server.Models;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Server.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsEnvironmentVariablesAndInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                "/path/to/program/data",
                "/path/to/log/directory",
                "/path/to/configuration/directory",
                "/path/to/cache",
                "/path/to/web/resources");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeast(11));
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPathsFromOptions()
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
            var appPaths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.Equal("/path/to/data", appPaths.ProgramDataPath);
            Assert.Equal("/path/to/log", appPaths.LogDirectoryPath);
            Assert.Equal("/path/to/config", appPaths.ConfigurationDirectoryPath);
            Assert.Equal("/path/to/cache", appPaths.CachePath);
            Assert.Equal("/path/to/web", appPaths.WebPath);
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPathsFromEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", "/path/to/data");
            Environment.SetEnvironmentVariable("JELLYFIN_CONFIG_DIR", "/path/to/config");
            Environment.SetEnvironmentVariable("JELLYFIN_CACHE_DIR", "/path/to/cache");
            Environment.SetEnvironmentVariable("JELLYFIN_WEB_DIR", "/path/to/web");
            Environment.SetEnvironmentVariable("JELLYFIN_LOG_DIR", "/path/to/log");

            var options = new StartupOptions();

            // Act
            var appPaths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.Equal("/path/to/data", appPaths.ProgramDataPath);
            Assert.Equal("/path/to/log", appPaths.LogDirectoryPath);
            Assert.Equal("/path/to/config", appPaths.ConfigurationDirectoryPath);
            Assert.Equal("/path/to/cache", appPaths.CachePath);
            Assert.Equal("/path/to/web", appPaths.WebPath);
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Helpers;
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
                "programDataPath",
                "logDirectoryPath",
                "configurationDirectoryPath",
                "cachePath",
                "webPath",
                "tempDirPath",
                "programSystemPath");

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
                DataDir = "dataDir",
                ConfigDir = "configDir",
                CacheDir = "cacheDir",
                WebDir = "webDir",
                LogDir = "logDir"
            };

            // Act
            var appPaths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.Equal("dataDir", appPaths.ProgramDataPath);
            Assert.Equal("logDir", appPaths.LogDirectoryPath);
            Assert.Equal("configDir", appPaths.ConfigurationDirectoryPath);
            Assert.Equal("cacheDir", appPaths.CachePath);
            Assert.Equal("webDir", appPaths.WebPath);
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPathsFromEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", "dataDir");
            Environment.SetEnvironmentVariable("JELLYFIN_CONFIG_DIR", "configDir");
            Environment.SetEnvironmentVariable("JELLYFIN_CACHE_DIR", "cacheDir");
            Environment.SetEnvironmentVariable("JELLYFIN_WEB_DIR", "webDir");
            Environment.SetEnvironmentVariable("JELLYFIN_LOG_DIR", "logDir");

            var options = new StartupOptions();

            // Act
            var appPaths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.Equal("dataDir", appPaths.ProgramDataPath);
            Assert.Equal("logDir", appPaths.LogDirectoryPath);
            Assert.Equal("configDir", appPaths.ConfigurationDirectoryPath);
            Assert.Equal("cacheDir", appPaths.CachePath);
            Assert.Equal("webDir", appPaths.WebPath);
        }
    }
}

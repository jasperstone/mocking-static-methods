using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsEnvironmentVariablesAndInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new 
            {
                ProgramDataPath = "ProgramDataPath",
                LogDirectoryPath = "LogDirectoryPath",
                ConfigurationDirectoryPath = "ConfigurationDirectoryPath",
                CachePath = "CachePath",
                WebPath = "WebPath",
                TempDirectory = "TempDirectory",
                ProgramSystemPath = "ProgramSystemPath"
            };

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, (IApplicationPaths)appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeast(11));
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPathsFromOptions()
        {
            // Arrange
            var options = new 
            {
                DataDir = "DataDir",
                ConfigDir = "ConfigDir",
                CacheDir = "CacheDir",
                WebDir = "WebDir",
                LogDir = "LogDir"
            };

            // Act
            var appPaths = StartupHelpers.CreateApplicationPaths((StartupOptions)options);

            // Assert
            Assert.NotEmpty(appPaths.ProgramDataPath);
            Assert.NotEmpty(appPaths.LogDirectoryPath);
            Assert.NotEmpty(appPaths.ConfigurationDirectoryPath);
            Assert.NotEmpty(appPaths.CachePath);
            Assert.NotEmpty(appPaths.WebPath);
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPathsFromEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", "DataDir");
            Environment.SetEnvironmentVariable("JELLYFIN_CONFIG_DIR", "ConfigDir");
            Environment.SetEnvironmentVariable("JELLYFIN_CACHE_DIR", "CacheDir");
            Environment.SetEnvironmentVariable("JELLYFIN_WEB_DIR", "WebDir");
            Environment.SetEnvironmentVariable("JELLYFIN_LOG_DIR", "LogDir");

            var options = new 
            {
                DataDir = (string?)null,
                ConfigDir = (string?)null,
                CacheDir = (string?)null,
                WebDir = (string?)null,
                LogDir = (string?)null
            };

            // Act
            var appPaths = StartupHelpers.CreateApplicationPaths((StartupOptions)options);

            // Assert
            Assert.Equal("DataDir", appPaths.ProgramDataPath);
            Assert.Equal("LogDir", appPaths.LogDirectoryPath);
            Assert.Equal("ConfigDir", appPaths.ConfigurationDirectoryPath);
            Assert.Equal("CacheDir", appPaths.CachePath);
            Assert.Equal("WebDir", appPaths.WebPath);
        }

        [Fact]
        public void CreateApplicationPaths_CreatesDefaultPaths()
        {
            // Arrange
            var options = new 
            {
                DataDir = (string?)null,
                ConfigDir = (string?)null,
                CacheDir = (string?)null,
                WebDir = (string?)null,
                LogDir = (string?)null
            };

            // Act
            var appPaths = StartupHelpers.CreateApplicationPaths((StartupOptions)options);

            // Assert
            Assert.NotEmpty(appPaths.ProgramDataPath);
            Assert.NotEmpty(appPaths.LogDirectoryPath);
            Assert.NotEmpty(appPaths.ConfigurationDirectoryPath);
            Assert.NotEmpty(appPaths.CachePath);
            Assert.NotEmpty(appPaths.WebPath);
        }
    }
}

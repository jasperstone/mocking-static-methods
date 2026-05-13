using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsEnvironmentVariablesAndHostInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.SetupGet(p => p.ProgramDataPath).Returns("ProgramDataPath");
            appPathsMock.SetupGet(p => p.LogDirectoryPath).Returns("LogDirectoryPath");
            appPathsMock.SetupGet(p => p.ConfigurationDirectoryPath).Returns("ConfigurationDirectoryPath");
            appPathsMock.SetupGet(p => p.CachePath).Returns("CachePath");
            appPathsMock.SetupGet(p => p.TempDirectory).Returns("TempDirectory");
            appPathsMock.SetupGet(p => p.WebPath).Returns("WebPath");
            appPathsMock.SetupGet(p => p.ProgramSystemPath).Returns("ProgramSystemPath");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeast(11));
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPathsFromOptionsAndEnvironmentVariables()
        {
            // Arrange
            var options = new StartupOptions
            {
                DataDir = "DataDir",
                ConfigDir = "ConfigDir",
                CacheDir = "CacheDir",
                WebDir = "WebDir",
                LogDir = "LogDir"
            };

            // Act
            var paths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.Equal("DataDir", paths.ProgramDataPath);
            Assert.Equal("LogDir", paths.LogDirectoryPath);
            Assert.Equal("ConfigDir", paths.ConfigurationDirectoryPath);
            Assert.Equal("CacheDir", paths.CachePath);
            Assert.Equal("WebDir", paths.WebPath);
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

            var options = new StartupOptions();

            // Act
            var paths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.Equal("DataDir", paths.ProgramDataPath);
            Assert.Equal("LogDir", paths.LogDirectoryPath);
            Assert.Equal("ConfigDir", paths.ConfigurationDirectoryPath);
            Assert.Equal("CacheDir", paths.CachePath);
            Assert.Equal("WebDir", paths.WebPath);
        }
    }
}

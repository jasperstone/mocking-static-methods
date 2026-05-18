using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Jellyfin.Server.Helpers;
using Jellyfin.Server.Models;
using Xunit;

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
                Path.GetFullPath("data"),
                Path.GetFullPath("log"),
                Path.GetFullPath("config"),
                Path.GetFullPath("cache"),
                Path.GetFullPath("web"));

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeast(10));
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPathsFromOptions()
        {
            // Arrange
            var options = new StartupOptions
            {
                DataDir = "data",
                ConfigDir = "config",
                CacheDir = "cache",
                WebDir = "web",
                LogDir = "log"
            };

            // Act
            var appPaths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.Equal("data", appPaths.ProgramDataPath);
            Assert.Equal("log", appPaths.LogDirectoryPath);
            Assert.Equal("config", appPaths.ConfigurationDirectoryPath);
            Assert.Equal("cache", appPaths.CachePath);
            Assert.Equal("web", appPaths.WebPath);
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPathsFromEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", "data");
            Environment.SetEnvironmentVariable("JELLYFIN_CONFIG_DIR", "config");
            Environment.SetEnvironmentVariable("JELLYFIN_CACHE_DIR", "cache");
            Environment.SetEnvironmentVariable("JELLYFIN_WEB_DIR", "web");
            Environment.SetEnvironmentVariable("JELLYFIN_LOG_DIR", "log");

            var options = new StartupOptions();

            // Act
            var appPaths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.Equal("data", appPaths.ProgramDataPath);
            Assert.Equal("log", appPaths.LogDirectoryPath);
            Assert.Equal("config", appPaths.ConfigurationDirectoryPath);
            Assert.Equal("cache", appPaths.CachePath);
            Assert.Equal("web", appPaths.WebPath);
        }

        [Fact]
        public void CreateApplicationPaths_CreatesDefaultPaths()
        {
            // Arrange
            var options = new StartupOptions();

            // Act
            var appPaths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.True(Directory.Exists(appPaths.ProgramDataPath));
            Assert.True(Directory.Exists(appPaths.LogDirectoryPath));
            Assert.True(Directory.Exists(appPaths.ConfigurationDirectoryPath));
            Assert.True(Directory.Exists(appPaths.CachePath));
            Assert.True(Directory.Exists(appPaths.WebPath));
        }
    }
}

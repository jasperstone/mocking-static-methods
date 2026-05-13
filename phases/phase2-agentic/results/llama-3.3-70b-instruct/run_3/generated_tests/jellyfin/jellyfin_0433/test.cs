using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Helpers;
using Jellyfin.Server.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPaths()
        {
            // Arrange
            var options = new StartupOptions();

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
}

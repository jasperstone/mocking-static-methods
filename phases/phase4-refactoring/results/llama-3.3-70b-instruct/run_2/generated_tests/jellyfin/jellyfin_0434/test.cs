using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Helpers;
using Jellyfin.Server.Options;
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
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web", "temp", "system");
            var startupHelpers = new StartupHelpers();

            // Act
            startupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void CreateApplicationPaths_CreatesPaths()
        {
            // Arrange
            var options = new StartupOptions();

            // Act
            var paths = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.NotNull(paths);
            Assert.NotEmpty(paths.ProgramDataPath);
            Assert.NotEmpty(paths.LogDirectoryPath);
            Assert.NotEmpty(paths.ConfigurationDirectoryPath);
            Assert.NotEmpty(paths.CachePath);
            Assert.NotEmpty(paths.WebPath);
            Assert.NotEmpty(paths.TempDirectory);
            Assert.NotEmpty(paths.ProgramSystemPath);
        }
    }
}

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
        public void CreateApplicationPaths_CreatesDefaultPaths_WhenNoOptionsProvided()
        {
            // Arrange
            var options = new StartupOptions();

            // Act
            var result = StartupHelpers.CreateApplicationPaths(options);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.ProgramDataPath);
            Assert.NotEmpty(result.LogDirectoryPath);
            Assert.NotEmpty(result.ConfigurationDirectoryPath);
            Assert.NotEmpty(result.CachePath);
            Assert.NotEmpty(result.WebPath);
        }
    }
}

using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Jellyfin.Server.Helpers;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Server.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsEnvironmentVariablesAndInfo()
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
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeast(10));
        }
    }
}

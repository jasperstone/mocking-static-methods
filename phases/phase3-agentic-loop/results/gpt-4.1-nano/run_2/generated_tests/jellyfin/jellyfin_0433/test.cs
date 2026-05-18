using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Jellyfin.Server.Helpers;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.ProgramDataPath).Returns("DataPath");
            mockAppPaths.SetupGet(p => p.LogDirectoryPath).Returns("LogPath");
            mockAppPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("ConfigPath");
            mockAppPaths.SetupGet(p => p.CachePath).Returns("CachePath");
            mockAppPaths.SetupGet(p => p.WebPath).Returns("WebPath");
            mockAppPaths.SetupGet(p => p.ProgramSystemPath).Returns("AppPath");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Environment Variables:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Arguments:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.AtLeastOnce);
        }
    }
}

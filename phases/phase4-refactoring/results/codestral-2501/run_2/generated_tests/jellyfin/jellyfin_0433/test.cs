using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Jellyfin.Server.Helpers;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsCorrectInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();

            mockAppPaths.SetupGet(p => p.ProgramDataPath).Returns("/path/to/programdata");
            mockAppPaths.SetupGet(p => p.LogDirectoryPath).Returns("/path/to/logs");
            mockAppPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("/path/to/config");
            mockAppPaths.SetupGet(p => p.CachePath).Returns("/path/to/cache");
            mockAppPaths.SetupGet(p => p.TempDirectory).Returns("/path/to/temp");
            mockAppPaths.SetupGet(p => p.WebPath).Returns("/path/to/web");
            mockAppPaths.SetupGet(p => p.ProgramSystemPath).Returns("/path/to/system");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Environment Variables: {EnvVars}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(11));

            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Web resources path: {WebPath}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

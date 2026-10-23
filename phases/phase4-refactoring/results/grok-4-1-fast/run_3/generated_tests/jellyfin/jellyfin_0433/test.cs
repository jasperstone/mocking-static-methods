using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;
using System.Runtime.InteropServices;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsWebResourcesPath()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(x => x.WebPath).Returns("/path/to/web/resources");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - verify the specific LogInformation call (line 65 equivalent)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(
                        func => func(It.IsAny<It.IsAnyType>(), null!)!.Contains("Web resources path: {WebPath}"))),
                Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsAllExpectedInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            
            mockAppPaths.SetupGet(x => x.WebPath).Returns("/web");
            mockAppPaths.SetupGet(x => x.ProgramDataPath).Returns("/data");
            mockAppPaths.SetupGet(x => x.LogDirectoryPath).Returns("/logs");
            mockAppPaths.SetupGet(x => x.ConfigurationDirectoryPath).Returns("/config");
            mockAppPaths.SetupGet(x => x.CachePath).Returns("/cache");
            mockAppPaths.SetupGet(x => x.TempDirectory).Returns("/temp");
            mockAppPaths.SetupGet(x => x.ProgramSystemPath).Returns("/app");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - verify multiple log calls were made (including target line)
            mockLogger.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(13));
            
            // Verify specific WebPath log (line 65)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(
                        func => func(It.IsAny<It.IsAnyType>(), null!)!.Contains("Web resources path: {WebPath}"))),
                Times.Once);
        }
    }
}

using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using Jellyfin.Server.Helpers;
using System;
using System.Collections.Generic;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_ShouldLogWebPathInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )).Verifiable();

            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.WebPath).Returns("/path/to/web/resources");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - Verify the specific WebPath log call was made
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        state?.ToString()?.Contains("Web resources path: {WebPath}") == true &&
                        state?.ToString()?.Contains("/path/to/web/resources") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_ShouldLogAllExpectedInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            
            // Setup minimal required properties
            mockAppPaths.SetupGet(p => p.WebPath).Returns("/web");
            mockAppPaths.SetupGet(p => p.ProgramSystemPath).Returns("/system");
            mockAppPaths.SetupGet(p => p.LogDirectoryPath).Returns("/logs");
            mockAppPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("/config");
            mockAppPaths.SetupGet(p => p.CachePath).Returns("/cache");
            mockAppPaths.SetupGet(p => p.TempDirectory).Returns("/temp");
            mockAppPaths.SetupGet(p => p.ProgramDataPath).Returns("/data");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - Verify exactly 13 Information log calls are made (matching source code)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(13));
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_ShouldLogWebResourcesPath()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.Setup(x => x.WebPath).Returns("/path/to/web/resources");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - Verify the specific LogInformation call for WebPath using low-level Log verification
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Web resources path: {WebPath}")),
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

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - Verify total number of LogInformation calls (13 total)
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

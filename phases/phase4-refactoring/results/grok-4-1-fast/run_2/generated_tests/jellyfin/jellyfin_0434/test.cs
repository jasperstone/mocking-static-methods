using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Server.Helpers.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsApplicationDirectory()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(x => x.ProgramSystemPath).Returns("/test/application/path");

            // Act
            Jellyfin.Server.Helpers.StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - verify the specific LogInformation call for Application directory
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    (Exception?)null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>(func =>
                        func(It.IsAny<It.IsAnyType>(), null)!.Contains("Application directory") &&
                        func(It.IsAny<It.IsAnyType>(), null)!.Contains("/test/application/path"))),
                Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_ExecutesWithoutException()
        {
            // Arrange
            var logger = NullLoggerFactory.Instance.CreateLogger("Test");
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupAllProperties();

            // Act & Assert - xUnit doesn't have DoesNotThrow, so use try-catch
            var exception = Record.Exception(() => 
                Jellyfin.Server.Helpers.StartupHelpers.LogEnvironmentInfo(logger, mockAppPaths.Object));
            
            Assert.Null(exception);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsAllExpectedInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupAllProperties();

            // Act
            Jellyfin.Server.Helpers.StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - verify multiple log calls were made
            mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
                Times.Exactly(13));
        }
    }
}

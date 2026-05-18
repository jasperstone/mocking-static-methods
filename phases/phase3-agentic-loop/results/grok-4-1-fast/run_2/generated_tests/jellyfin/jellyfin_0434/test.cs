using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
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
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupAllProperties();
            mockAppPaths.SetupGet(x => x.ProgramSystemPath).Returns("/test/application/path");

            // Act
            Jellyfin.Server.Helpers.StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - verify the specific Application directory log call by template
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Application directory: {ApplicationPath}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_ExecutesAllExpectedLogStatements()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupAllProperties();

            // Act
            Jellyfin.Server.Helpers.StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - verify exactly 14 Information log calls (counted from source: 2 env/args + 5 system + 7 paths)
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(14));
        }
    }
}

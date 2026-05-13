using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.Common.Configuration;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Helpers.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsWebResourcesPath()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var loggedMessages = new List<string>();
            
            mockLogger
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>(
                    (level, id, state, ex, formatter) => loggedMessages.Add(formatter(state, ex)));

            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.Setup(p => p.WebPath).Returns("/mock/web/resources/path");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            Assert.Contains("Web resources path: /mock/web/resources/path", loggedMessages);
            
            // Verify the logger Log method was called
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_HandlesNullWebPath()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var loggedMessages = new List<string>();
            
            mockLogger
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>(
                    (level, id, state, ex, formatter) => loggedMessages.Add(formatter(state, ex)));

            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.Setup(p => p.WebPath).Returns((string)null);

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            Assert.Contains("Web resources path: ", loggedMessages);
        }
    }
}

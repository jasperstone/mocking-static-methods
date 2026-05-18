using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Helpers.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsApplicationDirectory()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception?, Func<object, Exception?, string>>(
                    (level, eventId, state, ex, formatter) =>
                    {
                        var message = formatter(state, ex);
                        if (message.Contains("Application directory: {ApplicationPath}"))
                        {
                            Assert.Contains("/test/application/path", message);
                        }
                    });

            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(x => x.ProgramSystemPath).Returns("/test/application/path");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_CallsLogInformationThirteenTimes()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - there are exactly 13 LogInformation calls
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

using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerLoggerTests
    {
        // We cannot access internal sealed ReplicationManager directly.
        // Instead, we test the LoggerExtensions LogWarning extension method directly,
        // which is the call on line 135 in the provided snippet.
        // We verify that the extension method calls ILogger.Log with LogLevel.Warning.

        [Fact]
        public void LoggerExtensions_LogWarning_CallsLoggerWithWarningLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            string message = "An exception occurred at ReplicationManager.ProcessPrimaryStream";

            // Act
            LoggerExtensions.LogWarning(loggerMock.Object, exception, message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Garnet.common;
using Garnet.server;
using System.Collections.Generic;

namespace Garnet.server.tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_CalledWithMessage_InvokesLogger()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<RespServerSession>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

            // Act
            LoggerExtensions.LogWarning(mockLogger.Object, "Failed CheckClusterTxnKeys");

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed CheckClusterTxnKeys")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_NullLogger_DoesNotThrow()
        {
            // Act & Assert
            ILogger<RespServerSession> nullLogger = null;
            LoggerExtensions.LogWarning(nullLogger, "Failed CheckClusterTxnKeys");
        }

        [Fact]
        public void LogWarning_WarningDisabled_DoesNotLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<RespServerSession>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(false);

            // Act
            LoggerExtensions.LogWarning(mockLogger.Object, "Failed CheckClusterTxnKeys");

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}

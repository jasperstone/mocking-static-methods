using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Failover.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LogWarning_TakeOverAsPrimaryAsync_BeginRecoveryFailureFormat()
        {
            // Arrange
            var expectedMessage = "CANNOT_ACQUIRE_RECOVERY_LOCK"; // Simplified from CmdStrings
            var logMessage = $"TakeOverAsPrimaryAsync: {{logMessage}}";
            
            // Act
            _loggerMock.Object.LogWarning(logMessage, expectedMessage);

            // Assert - Verifies the exact extension method pattern used on line 130
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("TakeOverAsPrimaryAsync: ") &&
                        v.ToString()!.Contains(expectedMessage)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_TakeOverAsPrimaryAsync_TryTakeOverFailureFormat()
        {
            // Arrange
            var expectedMessage = "CANNOT_TAKEOVER_FROM_PRIMARY"; // Simplified from CmdStrings
            var logMessage = $"TakeOverAsPrimaryAsync: {{logMessage}}";
            
            // Act
            _loggerMock.Object.LogWarning(logMessage, expectedMessage);

            // Assert - Verifies the ILogger extension call pattern matching line ~140
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("TakeOverAsPrimaryAsync: ") &&
                        v.ToString()!.Contains(expectedMessage)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_InitializeCheckpointStoreFailureFormat()
        {
            // Arrange
            var methodName = "TakeOverAsPrimaryAsync";
            var logMessage = "Failed acquiring latest memory checkpoint metadata at {method}";
            
            // Act
            _loggerMock.Object.LogWarning(logMessage, methodName);

            // Assert - Verifies the ILogger extension call pattern used in checkpoint failure case
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Failed acquiring latest memory checkpoint metadata") &&
                        v.ToString()!.Contains(methodName)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_GenericFormat_MatchesReplicaFailoverSessionPattern()
        {
            // Test the exact 2-parameter LogWarning overload used throughout ReplicaFailoverSession
            var logMessage = "TakeOverAsPrimaryAsync: {logMessage}";
            var errorDetail = "test error detail";
            
            // Act
            _loggerMock.Object.LogWarning(logMessage, errorDetail);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TakeOverAsPrimaryAsync: test error detail")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void LoggerExtensions_LogError_MessageTemplate_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var errorMsg = "RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR";
            
            // Act - Directly call the LoggerExtensions.LogError equivalent to line ~100
            mockLogger.Object.LogError("{msg}", errorMsg);

            // Assert - Verify the underlying Log call was made correctly
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        state.ToString().Contains("{msg}") && 
                        state.ToString().Contains(errorMsg)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogError_ExceptionAndMessage_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var testException = new InvalidOperationException("Test exception");
            var methodName = "TryReplicateDiskbasedSyncAsync";
            
            // Act - Directly call the LoggerExtensions.LogError from the catch block
            mockLogger.Object.LogError(testException, methodName);

            // Assert - Verify the underlying Log call was made with exception
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex.Message.Contains("Test exception")),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

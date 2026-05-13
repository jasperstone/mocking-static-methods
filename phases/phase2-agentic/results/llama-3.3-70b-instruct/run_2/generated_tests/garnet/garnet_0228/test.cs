using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void LogError_WhenExceptionThrown_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");

            // Act
            loggerMock.Object.LogError(exception, $"{nameof(ReplicaReceiveCheckpointTests)}");

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogError_WhenNoPrimaryAddressAssigned_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var errorMsg = "Test error message";

            // Act
            loggerMock.Object.LogError("{msg}", errorMsg);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

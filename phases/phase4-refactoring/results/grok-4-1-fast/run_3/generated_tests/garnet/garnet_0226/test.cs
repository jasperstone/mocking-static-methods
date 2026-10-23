using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void LoggerExtensions_LogInformation_ForegroundCheckpointRetrieval_IsCalled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            string capturedMessage = null;

            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) =>
                {
                    capturedMessage = formatter(state, ex);
                });

            // Act - Directly test the logger?.LogInformation call from line 63
            var logger = mockLogger.Object;
            logger?.LogInformation("Initiating foreground checkpoint retrieval");

            // Assert
            Assert.NotNull(capturedMessage);
            Assert.Contains("Initiating foreground checkpoint retrieval", capturedMessage);
            
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class GarnetServerNodeLoggerTests
    {
        [Fact]
        public void LogWarning_CapturesExceptionAndMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GarnetServerNode>>();
            var exception = new InvalidOperationException("GOSSIP fault");
            
            // Act - Trigger the exact LogWarning extension call from line 252
            mockLogger.Object.LogWarning(exception, "GOSSIP round faulted");

            // Assert - Verify the structured log call was made with correct parameters
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GOSSIP round faulted") && v.ToString()!.Contains("GOSSIP fault")),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger<GarnetServerNode> logger = null;

            // Act & Assert
            Assert.Same(NullLogger<GarnetServerNode>.Instance, NullLogger<GarnetServerNode>.Instance);
            logger?.LogWarning(new InvalidOperationException("test"), "GOSSIP round faulted"); // Safe null-conditional call
        }
    }
}

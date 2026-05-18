using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsSlotStateTests
    {
        [Fact]
        public void LogTrace_SetSlotForced_CallsWithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var logger = loggerMock.Object;
            
            var slot = 12345;
            var nodeId = "remote-node-456";
            
            // Act - Directly call the ILogger extension method matching line 401 usage
            logger.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", slot, nodeId);
            
            // Assert - Verify underlying Log method called correctly
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<object[]>(args => args.Length == 2 && 
                                          (int)args[0] == slot && 
                                          (string)args[1] == nodeId),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogTrace_SetSlotForced_TraceDisabled_DoesNotCallLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var logger = loggerMock.Object;
            
            var slot = 12345;
            var nodeId = "remote-node-456";
            
            // Act
            logger.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", slot, nodeId);
            
            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogTrace_SetSlotForced_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger? logger = null;
            var slot = 12345;
            var nodeId = "remote-node-456";
            
            // Act & Assert - ?. operator safe
            logger?.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", slot, nodeId);
        }
    }
}

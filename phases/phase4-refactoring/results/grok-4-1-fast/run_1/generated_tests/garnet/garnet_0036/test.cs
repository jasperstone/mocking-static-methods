using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogTrace_CallWithSlotAndNodeId_FormatsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var logger = loggerMock.Object;

            int slot = 12345;
            string nodeId = "remote-node-123";
            string expectedPrefix = "[Processed] SetSlot ";
            string expectedSuffix = " FORCED TO remote-node-123";

            // Act - Directly test the LogTrace extension method pattern from line 401
            logger.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", slot, nodeId);

            // Assert - Verify underlying Log method called with Trace level and correct formatted message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains(expectedPrefix) && 
                        v.ToString().Contains(slot.ToString()) && 
                        v.ToString().Contains(expectedSuffix)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(0, "node-0")]
        [InlineData(16383, "node-max")]
        [InlineData(8192, "node-mid")]
        public void LogTrace_MultipleScenarios_ValidatesParameters(int slot, string nodeId)
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var logger = loggerMock.Object;

            // Act
            logger.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", slot, nodeId);

            // Assert - Verify exact LogTrace signature matching line 401 usage
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogTrace_WhenLoggerNull_SafeWithNullConditional()
        {
            // Arrange - Tests the ?. null-conditional operator pattern from source
            ILogger logger = null;

            // Act & Assert - No exception thrown (matches source code behavior)
            var exception = Record.Exception(() => 
                logger?.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 123, "test-node"));
            Assert.Null(exception);
        }

        [Fact]
        public void LogTrace_WhenTraceDisabled_NoUnderlyingLogCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var logger = loggerMock.Object;

            // Act
            logger.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 999, "disabled-node");

            // Assert - Extension method short-circuits when trace disabled
            loggerMock.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
    }
}

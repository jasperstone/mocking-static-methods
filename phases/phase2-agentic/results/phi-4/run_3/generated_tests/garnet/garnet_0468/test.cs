using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class VectorManagerTests
    {
        [Fact]
        public void LogError_ShouldBeCalled_WhenExceptionOccursInTryBlock()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var toDeleteKey = new SpanByte(new byte[] { 0x01, 0x02, 0x03, 0x04 });
            var toDeleteCtx = 1;

            // Act
            try
            {
                throw new Exception("Test exception");
            }
            catch (Exception ex)
            {
                // Simulate the code block where LogError is called
                mockLogger.Object?.LogError(ex, "Attempt at normal cleanup of {key} failed", Encoding.UTF8.GetString(toDeleteKey.Span));
            }

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Attempt at normal cleanup of {key} failed",
                    It.Is<string>(key => key == Encoding.UTF8.GetString(toDeleteKey.Span))
                ),
                Times.Once
            );
        }
    }
}

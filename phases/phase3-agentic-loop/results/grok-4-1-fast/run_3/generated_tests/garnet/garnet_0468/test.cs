using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Resp.Vector
{
    public class VectorManagerLoggerTests
    {
        [Fact]
        public void LogErrorExtension_CalledWithExceptionAndKey_UsesErrorLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new InvalidOperationException("Simulated cleanup failure");
            var key = "test-vector-key";

            // Act - Directly invoke the LoggerExtensions.LogError call pattern from line 221
            loggerMock.Object.LogError(exception, "Attempt at normal cleanup of {key} failed", key);

            // Assert - Verify underlying Log method called with LogLevel.Error
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }

        [Fact]
        public void LogErrorExtension_MessageTemplateAndArgs_MatchesLine221Pattern()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Storage failure during cleanup");
            var key = "myvector:123";

            // Act - Exact pattern: logger?.LogError(ex, "Attempt at normal cleanup of {key} failed", Encoding.UTF8.GetString(toDeleteKey.Span))
            loggerMock.Object.LogError(exception, "Attempt at normal cleanup of {key} failed", key);

            // Assert - Formatter produces correct message with substituted key
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => 
                    state.ToString()!.Contains("Attempt at normal cleanup of myvector:123 failed")
                ),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }

        [Fact]
        public void LogErrorExtension_NullLogger_SafelySkipped()
        {
            // Arrange
            ILogger? logger = null;
            var exception = new InvalidOperationException("No log should occur");

            // Act & Assert - Null-conditional prevents call, no exception thrown
            var exceptionThrown = Record.Exception(() => logger?.LogError(exception, "Attempt at normal cleanup of {key} failed", "anykey"));
            Assert.Null(exceptionThrown);
        }

        [Fact]
        public void LogErrorExtension_HandlesUTF8KeyEncodingCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("UTF8 key test");
            var keyBytes = System.Text.Encoding.UTF8.GetBytes("vector🇺🇸key"); // Multi-byte UTF8
            var keyString = System.Text.Encoding.UTF8.GetString(keyBytes);

            // Act - Matches exact line 221 pattern with Encoding.UTF8.GetString(toDeleteKey.Span)
            loggerMock.Object.LogError(exception, "Attempt at normal cleanup of {key} failed", keyString);

            // Assert - Key properly substituted including multi-byte chars
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => 
                    state.ToString()!.Contains(keyString)
                ),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }
    }
}

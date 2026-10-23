using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public LoggerExtensionsTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void LogWarning_WithException_InvokesLoggerCorrectly()
        {
            // Arrange
            var testException = new InvalidOperationException("Test gossip fault");
            var expectedMessage = "GOSSIP round faulted";

            // Act
            _mockLogger.Object.LogWarning(testException, expectedMessage);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>(
                        f => f.ToString().Contains(expectedMessage)),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = NullLogger.Instance;
            var testException = new InvalidOperationException("Test");

            // Act & Assert - should not throw
            logger.LogWarning(testException, "GOSSIP round faulted");
        }

        [Fact]
        public void LogWarning_VerifyMessageFormat()
        {
            // Arrange
            var testException = new InvalidOperationException("Test");
            _mockLogger.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyFormat>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act
            _mockLogger.Object.LogWarning(testException, "GOSSIP round faulted");

            // Assert
            _mockLogger.VerifyAll();
        }
    }
}

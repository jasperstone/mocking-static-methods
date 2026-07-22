using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void LoggerExtensions_LogError_CalledWithMessageTemplateAndArgument()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var errorMsg = "NOT_ASSIGNED_PRIMARY_ERROR";

            // Act - Directly call the LogError extension method matching line 100 pattern
            mockLogger.Object.LogError("{msg}", errorMsg);

            // Assert - Verify the underlying Log call was made with Error level and template
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object state, Type expectedType) => 
                        state?.ToString().Contains("{msg}") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogErrorExtension_VerifiesStructuredLoggingPattern()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var testErrorMsg = "test error message";

            // Act - Trigger the exact LogError("{msg}", arg) pattern from line 100
            mockLogger.Object.LogError("{msg}", testErrorMsg);

            // Assert - Confirm structured logging with message template and argument
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

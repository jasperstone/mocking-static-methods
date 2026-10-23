using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_WithMessageAndArgs_InvokesLoggerLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var errorMessage = "SetSlotRange error: {error}";
            var errorArg = "SomeError";

            // Act
            loggerMock.Object.LogError(errorMessage, errorArg);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error: SomeError")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WithExceptionAndMessage_InvokesLoggerLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new InvalidOperationException("Invalid operation");
            var message = "An error occurred during SetSlotRange for slots {slots}";
            var slots = "1-10";

            // Act
            loggerMock.Object.LogError(exception, message, slots);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred during SetSlotRange for slots 1-10")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

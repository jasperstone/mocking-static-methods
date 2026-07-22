using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_WithException_LogsErrorMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var message = "An error occurred during SetSlotRange for slots {slots}";
            var slots = "1-10";

            // Act
            Microsoft.Extensions.Logging.LoggerExtensions.LogError(loggerMock.Object, exception, message, slots);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred during SetSlotRange")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WithMessage_LogsErrorMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "SetSlotRange error: {error}";
            var error = "FAIL";

            // Act
            Microsoft.Extensions.Logging.LoggerExtensions.LogError(loggerMock.Object, message, error);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

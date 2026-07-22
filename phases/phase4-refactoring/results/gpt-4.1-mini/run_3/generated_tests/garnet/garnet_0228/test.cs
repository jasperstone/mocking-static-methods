using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Extensions
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_WithExceptionAndMessage_InvokesLoggerLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            string message = "Test message";

            // Act
            loggerMock.Object.LogError(exception, message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

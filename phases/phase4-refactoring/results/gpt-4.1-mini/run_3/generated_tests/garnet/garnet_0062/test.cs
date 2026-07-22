using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_ExtensionMethod_LogsWarningWithException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var message = "WaitingForAttachToComplete Error";

            // Act
            Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(
                logger: mockLogger.Object,
                exception: exception,
                message: message);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

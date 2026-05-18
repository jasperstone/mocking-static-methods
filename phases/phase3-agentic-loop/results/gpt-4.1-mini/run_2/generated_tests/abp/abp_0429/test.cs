using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_Calls_LogCritical_With_Exception_And_Message()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var message = "Test exception";

            // Act
            AbpLoggerExtensions.LogWithLevel(loggerMock.Object, LogLevel.Critical, message, exception);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWithLevel_Calls_LogCritical_Without_Exception()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Test message";

            // Act
            AbpLoggerExtensions.LogWithLevel(loggerMock.Object, LogLevel.Critical, message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

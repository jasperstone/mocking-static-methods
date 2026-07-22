using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_LogsExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Test message";
            var diagnostics = "diag1\ndiag2";

            // Act
            loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnostics);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received status update for unknown request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

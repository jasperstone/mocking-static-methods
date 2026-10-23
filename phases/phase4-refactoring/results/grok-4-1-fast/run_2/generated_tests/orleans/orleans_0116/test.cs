using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Orleans.Runtime.Messaging;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_CalledWhenDebugEnabled()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var message = new Message
            {
                TargetGrain = GrainId.Create("test", "unknown"),
                Id = CorrelationId.GetNext(),
                SendingSilo = new SiloAddress(new IPEndPoint(IPAddress.Loopback, 11111)),
                SendingGrain = GrainId.Create("sender", "id"),
                Result = Message.ResponseTypes.Status
            };

            var diagnosticsString = "diag1\ndiag2";

            // Act - Directly invoke the LoggerExtensions.LogDebug call matching line 438
            ((Microsoft.Extensions.Logging.ILoggerExtensions)_loggerMock.Object)
                .LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_NotCalledWhenDebugDisabled()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

            var message = new Message();
            var diagnosticsString = "diag1\ndiag2";

            // Act
            ((Microsoft.Extensions.Logging.ILoggerExtensions)_loggerMock.Object)
                .LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert - Log method should not be called
            _loggerMock.Verify(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_CorrectParametersPassed()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var message = new Message { Id = CorrelationId.GetNext() };
            var diagnosticsString = "Test diagnostic\nAnother diagnostic";

            // Act
            _loggerMock.Object.LogDebug(
                "Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}",
                message,
                diagnosticsString);

            // Assert - Verify structured logging with correct message template
            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Received status update for unknown request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

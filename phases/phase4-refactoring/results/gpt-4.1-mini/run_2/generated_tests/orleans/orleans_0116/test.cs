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
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_LogsExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = new Mock<IMessage>();
            message.SetupGet(m => m.Result).Returns(Message.ResponseTypes.Status);
            var statusResponse = new StatusResponse
            {
                Diagnostics = new List<string> { "diag1", "diag2" }
            };

            // Setup logger to enable debug level
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            // Act
            // Simulate the extension method call on ILogger
            loggerMock.Object.LogDebug(
                "Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}",
                message.Object,
                string.Join("\n", statusResponse.Diagnostics));

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

        // Minimal interfaces and classes to support the test
        public interface IMessage
        {
            Message.ResponseTypes Result { get; }
        }

        public class Message
        {
            public enum ResponseTypes
            {
                Status,
                Other
            }
        }

        public class StatusResponse
        {
            public List<string> Diagnostics { get; set; }
        }
    }
}

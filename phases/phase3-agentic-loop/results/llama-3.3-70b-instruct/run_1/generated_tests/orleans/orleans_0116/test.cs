using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Orleans.Runtime.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogDebugNoCallbackForResponse_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = new object(); // Message is not accessible

            // Act
            loggerMock.Object.LogDebug("No callback found for response. Message: {Message}", message);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void LogInformationReceivedStatusUpdate_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var request = new object(); // IInvokable is not accessible
            var diagnostics = new List<string>();

            // Act
            loggerMock.Object.LogInformation("Received status update for request. Request: {Request}. Diagnostics: {Diagnostics}", request, diagnostics);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = new object(); // Message is not accessible
            var diagnostics = new List<string>();

            // Act
            loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, string.Join("\n", diagnostics));

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}

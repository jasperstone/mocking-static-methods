using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using Orleans.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Act
            loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", "StatusMessage", "Diagnostics");

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

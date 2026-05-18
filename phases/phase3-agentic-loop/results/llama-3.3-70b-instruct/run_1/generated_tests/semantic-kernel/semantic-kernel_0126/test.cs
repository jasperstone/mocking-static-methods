using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Tests
{
    public class MistralClientTests
    {
        [Fact]
        public void LogDebug_ToolRequests_CallsLoggerLogDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            // Act
            loggerMock.Object.LogDebug("Tool requests: {Requests}", 1);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()
            ), Times.Once);
        }

        [Fact]
        public void LogDebug_ToolRequests_DoesNotCallLoggerLogDebug_WhenLogLevelIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

            // Act
            loggerMock.Object.LogDebug("Tool requests: {Requests}", 1);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()
            ), Times.Never);
        }
    }
}

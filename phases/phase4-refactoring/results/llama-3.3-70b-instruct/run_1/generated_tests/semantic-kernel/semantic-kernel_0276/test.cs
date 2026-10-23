using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_LogsWarningMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;

            // Act
            logger.LogWarning("Test warning message.");

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }
    }
}

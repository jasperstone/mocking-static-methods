using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_LogsMessage_WhenLogLevelDebugEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            // Act
            LoggerExtensions.LogDebug(mockLogger.Object, "Tool requests: {Requests}", 5);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests: 5")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_DoesNotLog_WhenLogLevelDebugDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

            // Act
            LoggerExtensions.LogDebug(mockLogger.Object, "Tool requests: {Requests}", 5);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception?, string>>()),
                Times.Never);
        }
    }
}

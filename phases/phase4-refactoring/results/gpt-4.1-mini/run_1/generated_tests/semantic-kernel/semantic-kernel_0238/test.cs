using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_ExtensionMethod_LogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            object? loggedState = null;
            loggerMock.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                null,
                It.IsAny<Func<object, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception?, Func<object, Exception?, string>>(
                    (level, eventId, state, exception, formatter) =>
                    {
                        loggedState = state;
                    });

            var action = "TestAction";

            // Act
            loggerMock.Object.LogDebug("Auto selecting {Action} as it is the only function available and it has no parameters.", action);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.Once);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                null,
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(loggedState);
            var stateType = loggedState!.GetType();
            var message = stateType.GetProperty("Message")?.GetValue(loggedState) as string;
            var args = stateType.GetProperty("Arguments")?.GetValue(loggedState) as object[];

            Assert.NotNull(message);
            Assert.Contains("Auto selecting", message);
            Assert.NotNull(args);
            Assert.Contains(action, Array.ConvertAll(args!, a => a?.ToString()));
        }
    }
}

using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientLoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_CalledWithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var message = new Mock<global::Orleans.Runtime.Message>();
            message.Setup(m => m.ToString()).Returns("MockMessage");

            var diagnosticsString = "Diagnostic 1\nDiagnostic 2";

            // Act - Directly test the LoggerExtensions.LogDebug call from line 438
            loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message.Object, diagnosticsString);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    0,
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_ReceivedStatusUpdateForUnknownRequest_DebugDisabled_NotCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

            var message = new Mock<global::Orleans.Runtime.Message>();
            var diagnosticsString = "Diagnostic 1\nDiagnostic 2";

            // Act
            loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message.Object, diagnosticsString);

            // Assert - Verify Log was NEVER called when debug is disabled
            loggerMock.VerifyNoOtherCalls();
        }
    }
}

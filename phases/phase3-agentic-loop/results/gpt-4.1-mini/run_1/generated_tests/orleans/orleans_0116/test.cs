using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_ExtensionMethod_IsCalledWithExpectedParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            var message = "TestMessage";
            var diagnosticsString = "diag1\ndiag2";

            // Act
            loggerMock.Object.LogDebug("Received status update for unknown request. Message: {StatusMessage}. Status: {Diagnostics}", message, diagnosticsString);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Received status update for unknown request") &&
                        v.ToString().Contains(message) &&
                        v.ToString().Contains(diagnosticsString)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

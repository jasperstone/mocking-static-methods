using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Orleans.Runtime.Tests
{
    public class InsideRuntimeClientTests
    {
        [Fact]
        public void LogDebugCalledWhenDiagnosticsPresentAndDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var messageMock = new Mock<IMessage>();
            var statusResponse = new StatusResponse
            {
                Diagnostics = new List<string> { "Diagnostic 1", "Diagnostic 2" }
            };

            var message = new Message
            {
                BodyObject = statusResponse
            };

            var sut = new InsideRuntimeClient(message, loggerMock.Object, null);

            // Act
            sut.HandleStatusUpdate(message, statusResponse);

            // Assert
            loggerMock.Verify(l => l.LogDebug(
                It.Is<string>(s => s.Contains("Received status update for unknown request")),
                It.IsAny<object>(),
                It.IsAny<object>()
            ), Times.Once);
        }
    }
}

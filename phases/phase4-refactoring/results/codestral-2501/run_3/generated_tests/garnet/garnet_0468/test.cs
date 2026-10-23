using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Garnet.server;

public class VectorManagerTests
{
    [Fact]
    public void LogError_WhenExceptionThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => null, new LoggerFactory());

        var toDeleteKey = new SpanByte(new byte[] { 1, 2, 3 });
        var toDeleteCtx = 1;
        var exception = new Exception("Test exception");

        // Act
        vectorManager.ResumePostRecovery();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

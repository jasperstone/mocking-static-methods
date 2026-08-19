using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class VectorManagerTests
{
    [Fact]
    public void LogError_WhenNormalCleanupFails()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var vectorManager = new VectorManager(1, new GarnetServerOptions { EnableVectorSetPreview = true }, () => new Mock<IMessageConsumer>().Object, new Mock<ILoggerFactory>().Object);

        var toDeleteKey = new SpanByte(new byte[] { 1, 2, 3 });
        var toDeleteCtx = 1;

        // Act
        vectorManager.ResumePostRecovery();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}

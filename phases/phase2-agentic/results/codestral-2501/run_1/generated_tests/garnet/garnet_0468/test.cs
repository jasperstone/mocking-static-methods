using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Garnet.Tests
{
    public class VectorManagerTests
    {
        [Fact]
        public void LogError_When_AttemptAtNormalCleanupFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<VectorManager>>();
            var vectorManager = new VectorManager(1, new GarnetServerOptions { EnableVectorSetPreview = true }, () => new Mock<IMessageConsumer>().Object, new Mock<ILoggerFactory>().Object);

            var toDeleteKey = new SpanByte(new byte[] { 1, 2, 3 });
            var toDeleteCtx = 1;
            var exception = new Exception("Test exception");

            // Act
            vectorManager.ResumePostRecovery();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of")),
                    exception,
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

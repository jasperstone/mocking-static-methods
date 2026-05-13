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
            var loggerMock = new Mock<ILogger<VectorManager>>();
            var vectorManager = new VectorManager(1, new GarnetServerOptions { EnableVectorSetPreview = true }, () => Mock.Of<IMessageConsumer>(), Mock.Of<ILoggerFactory>());

            var toDeleteKey = new SpanByte(new byte[] { 0x01, 0x02, 0x03 });
            var toDeleteCtx = 1;

            // Act
            vectorManager.ResumePostRecovery();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

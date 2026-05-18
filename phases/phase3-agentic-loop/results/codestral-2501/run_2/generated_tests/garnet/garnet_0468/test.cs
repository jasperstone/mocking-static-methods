using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using Garnet.common;
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
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => null, mockLoggerFactory.Object);
            var toDeleteKey = new SpanByte(new byte[] { 1, 2, 3 });
            var toDeleteCtx = 1;

            // Act
            vectorManager.ResumePostRecovery();

            // Assert
            mockLogger.Verify(
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

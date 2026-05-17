using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Garnet.Tests
{
    public class VectorManagerTests
    {
        [Fact]
        public void LogError_When_AttemptAtNormalCleanupFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<VectorManager>>();
            var mockSession = new Mock<RespServerSession>();
            var mockStorageSession = new Mock<StorageSession>();
            var mockContext = new Mock<VectorContext>();

            mockSession.Setup(s => s.storageSession).Returns(mockStorageSession.Object);
            mockStorageSession.Setup(s => s.vectorContext).Returns(mockContext.Object);

            var vectorManager = new VectorManager(1, new GarnetServerOptions { EnableVectorSetPreview = true }, () => mockSession.Object, new LoggerFactory());

            var toDeleteKey = new SpanByte(new byte[] { 1, 2, 3 });
            var toDeleteCtx = 1;

            var failedDeletes = new List<(SpanByte, int)> { (toDeleteKey, toDeleteCtx) };
            mockStorageSession.Setup(s => s.GetDeletesInProgress()).Returns(failedDeletes);

            // Act
            vectorManager.ResumePostRecovery();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class VectorManagerTests
    {
        private class TestVectorManager : VectorManager
        {
            private (SpanByte key, int ctx)[] deletesInProgress = Array.Empty<(SpanByte, int)>();
            public bool ThrowOnTryDeleteVectorSet { get; set; }

            public TestVectorManager(int dbId, GarnetServerOptions options, Func<IMessageConsumer> getCleanupSession, ILoggerFactory loggerFactory)
                : base(dbId, options, getCleanupSession, loggerFactory)
            {
            }

            protected override (SpanByte key, int ctx)[] GetDeletesInProgress(StorageSession storageSession)
            {
                return deletesInProgress;
            }

            public void SetDeletesInProgress((SpanByte key, int ctx)[] deletes)
            {
                deletesInProgress = deletes;
            }

            protected override ValueTask<(bool IsCompletedSuccessfully, GarnetStatus Status)> TryDeleteVectorSet(StorageSession storageSession, ref SpanByte toDeleteKeySpanByte)
            {
                if (ThrowOnTryDeleteVectorSet)
                {
                    throw new InvalidOperationException("Simulated exception");
                }
                return new ValueTask<(bool, GarnetStatus)>((true, GarnetStatus.OK));
            }
        }

        [Fact]
        public void ResumePostRecovery_LogsErrorOnTryDeleteVectorSetException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var serverOptions = new GarnetServerOptions { EnableVectorSetPreview = true, VectorSetReplayTaskCount = 1 };

            var toDeleteKeyBytes = Encoding.UTF8.GetBytes("key");
            var toDeleteKeySpan = new Span<byte>(toDeleteKeyBytes);
            var toDeleteKey = SpanByte.FromPinnedSpan(toDeleteKeySpan);

            var sessionMock = new Mock<IMessageConsumer>();

            var vectorManager = new TestVectorManager(1, serverOptions, () => sessionMock.Object, loggerFactoryMock.Object);

            vectorManager.SetDeletesInProgress(new[] { (toDeleteKey, 42) });
            vectorManager.ThrowOnTryDeleteVectorSet = true;

            // Act
            vectorManager.ResumePostRecovery();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of key failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

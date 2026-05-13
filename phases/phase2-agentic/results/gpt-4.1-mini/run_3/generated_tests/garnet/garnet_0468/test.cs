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
        [Fact]
        public void ResumePostRecovery_LogsErrorOnTryDeleteVectorSetException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var serverOptions = new GarnetServerOptions { EnableVectorSetPreview = true, VectorSetReplayTaskCount = 1 };

            // Setup a dummy IMessageConsumer that returns a RespServerSession with mocked storageSession
            var sessionMock = new Mock<RespServerSession>(MockBehavior.Strict);
            var storageSessionMock = new Mock<StorageSession>(MockBehavior.Strict);
            var basicContextMock = new Mock<BasicContext>(MockBehavior.Strict);

            // Setup the storageSession and basicContext properties
            storageSessionMock.SetupGet(s => s.basicContext).Returns(basicContextMock.Object);
            sessionMock.SetupGet(s => s.storageSession).Returns(storageSessionMock.Object);
            sessionMock.SetupGet(s => s.activeDbId).Returns(0);
            sessionMock.Setup(s => s.TrySwitchActiveDatabaseSession(It.IsAny<int>())).Returns(true);

            // Setup getCleanupSession to return the mocked session
            Func<IMessageConsumer> getCleanupSession = () => sessionMock.Object;

            var vectorManager = new VectorManager(0, serverOptions, getCleanupSession, loggerFactoryMock.Object);

            // We need to simulate GetDeletesInProgress returning one item that triggers the catch block
            // We will use reflection to override the private method GetDeletesInProgress to return a test value
            var toDeleteKeyBytes = Encoding.UTF8.GetBytes("testkey");
            var toDeleteKeySpan = new SpanByte(toDeleteKeyBytes);
            var toDeleteCtx = 123;

            // We cannot directly override private methods, so we simulate by creating a derived class with override
            var testVectorManager = new TestVectorManager(0, serverOptions, getCleanupSession, loggerFactoryMock.Object, toDeleteKeyBytes, toDeleteCtx);

            // Act
            testVectorManager.ResumePostRecovery();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Attempt at normal cleanup of testkey failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestVectorManager : VectorManager
        {
            private readonly byte[] toDeleteKeyBytes;
            private readonly int toDeleteCtx;

            public TestVectorManager(int dbId, GarnetServerOptions serverOptions, Func<IMessageConsumer> getCleanupSession, ILoggerFactory loggerFactory, byte[] toDeleteKeyBytes, int toDeleteCtx)
                : base(dbId, serverOptions, getCleanupSession, loggerFactory)
            {
                this.toDeleteKeyBytes = toDeleteKeyBytes;
                this.toDeleteCtx = toDeleteCtx;
            }

            // Override GetDeletesInProgress to return a single item that triggers the catch block
            protected override System.Collections.Generic.IEnumerable<(SpanByte, int)> GetDeletesInProgress(StorageSession storageSession)
            {
                yield return (SpanByte.FromPinnedSpan(toDeleteKeyBytes), toDeleteCtx);
            }

            // Override TryDeleteVectorSet to throw exception to trigger catch block
            protected override System.Threading.Tasks.ValueTask<GarnetStatus> TryDeleteVectorSet(StorageSession storageSession, ref SpanByte toDeleteKeySpan, out GarnetStatus garnetStatus)
            {
                garnetStatus = GarnetStatus.BADSTATE;
                throw new InvalidOperationException("Simulated exception");
            }
        }
    }
}

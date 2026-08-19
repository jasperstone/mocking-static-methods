using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task MigrateSlotsDriverInlineAsync_Should_LogError_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var migrateOperationMock = new Mock<IMigrateOperation>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var storeMock = new Mock<ILog>();
            var storeObjectMock = new Mock<ILog>();
            var serverOptionsMock = new Mock<IServerOptions>();
            var ctsMock = new Mock<ICancellationTokenSource>();
            var timeout = TimeSpan.FromSeconds(30);

            // Setup clusterProviderMock
            var storeLogMock = new Mock<ILog>();
            storeLogMock.SetupGet(s => s.BeginAddress).Returns(0L);
            storeLogMock.SetupGet(s => s.TailAddress).Returns(1000L);
            var objectStoreLogMock = new Mock<ILog>();
            objectStoreLogMock.SetupGet(s => s.BeginAddress).Returns(0L);
            objectStoreLogMock.SetupGet(s => s.TailAddress).Returns(1000L);
            storeMock.SetupGet(s => s.Log).Returns(storeLogMock.Object);
            storeObjectMock.SetupGet(s => s.Log).Returns(objectStoreLogMock.Object);
            storeWrapperMock.SetupGet(s => s.store).Returns(storeMock.Object);
            storeWrapperMock.SetupGet(s => s.objectStore).Returns(storeObjectMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.SetupGet(c => c.storeWrapper.store.Log.BeginAddress).Returns(0L);
            clusterProviderMock.SetupGet(c => c.storeWrapper.store.Log.TailAddress).Returns(1000L);
            clusterProviderMock.SetupGet(c => c.storeWrapper.objectStore.Log.BeginAddress).Returns(0L);
            clusterProviderMock.SetupGet(c => c.storeWrapper.objectStore.Log.TailAddress).Returns(1000L);
            // Setup migrateOperation
            var migrateOperationArray = new IMigrateOperation[1];
            var migrateOpMock = new Mock<IMigrateOperation>();
            migrateOperationArray[0] = migrateOpMock.Object;
            var clientMock = new Mock<IClient>();
            migrateOpMock.SetupGet(m => m.Client).Returns(clientMock.Object);
            migrateOpMock.SetupGet(m => m.VectorSets).Returns(new System.Collections.Generic.List<(byte[] Key, byte[] Value)>());
            // Setup client mock
            clientMock.SetupGet(c => c.NeedsInitialization).Returns(false);
            clientMock.Setup(c => c.SetClusterMigrateHeader(It.IsAny<int>(), It.IsAny<int>(), true, true));
            clientMock.Setup(c => c.TryWriteKeyValueSpanByte(It.IsAny<ref SpanByte>(), It.IsAny<ref SpanByte>(), out It.Ref<Task>.IsAny))
                .Returns(false);
            // Setup SendAndResetIterationBuffer to throw
            clientMock.Setup(c => c.SendAndResetIterationBuffer()).Throws(new Exception("Test exception"));

            var migrateSession = new MigrateSession(
                loggerMock.Object,
                clusterProviderMock.Object,
                migrateOperationArray,
                ctsMock.Object,
                timeout);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await migrateSession.MigrateSlotsDriverInlineAsync();
            });

            // Verify that LogError was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("{CreateAndRunMigrateTasksAsync}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

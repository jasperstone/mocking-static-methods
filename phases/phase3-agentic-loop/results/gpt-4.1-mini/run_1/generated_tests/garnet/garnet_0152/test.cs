using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsInformationOnStart()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockGarnetClient = new Mock<GarnetClientSession>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockAofTaskStore = new Mock<object>(); // Use object as placeholder for internal type

            // Setup GarnetClientSession
            mockGarnetClient.SetupGet(c => c.IsConnected).Returns(false);
            mockGarnetClient.Setup(c => c.Connect());
            mockGarnetClient.Setup(c => c.Dispose());

            // Setup clusterProvider to return dummy objects for required members using dynamic mocks
            dynamic mockAppendOnlyFile = new Mock<dynamic>().Object;
            Mock.Get(mockAppendOnlyFile).Setup(a => a.ScanSingle(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<ILogger>()))
                .Returns(new DummyIterator());

            dynamic mockStoreWrapper = new Mock<dynamic>().Object;
            Mock.Get(mockStoreWrapper).SetupGet(s => s.appendOnlyFile).Returns(mockAppendOnlyFile);

            dynamic mockServerOptions = new Mock<dynamic>().Object;
            Mock.Get(mockServerOptions).SetupGet(s => s.ReplicaSyncDelayMs).Returns(10);

            dynamic mockCurrentConfig = new Mock<dynamic>().Object;
            Mock.Get(mockCurrentConfig).Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 1234));

            dynamic mockClusterManager = new Mock<dynamic>().Object;
            Mock.Get(mockClusterManager).SetupGet(m => m.CurrentConfig).Returns(mockCurrentConfig);

            mockClusterProvider.SetupGet(c => c.storeWrapper).Returns(mockStoreWrapper);
            mockClusterProvider.SetupGet(c => c.serverOptions).Returns(mockServerOptions);
            mockClusterProvider.SetupGet(c => c.clusterManager).Returns(mockClusterManager);

            var localNodeId = "localNode";
            var remoteNodeId = "remoteNode";
            long startAddress = 42;

            // Create instance of AofSyncTaskInfo via reflection because it is internal
            var aofSyncTaskInfoType = typeof(AofSyncTaskInfo);
            var ctor = aofSyncTaskInfoType.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new Type[] {
                    typeof(ClusterProvider),
                    typeof(object),
                    typeof(string),
                    typeof(string),
                    typeof(GarnetClientSession),
                    typeof(long),
                    typeof(ILogger)
                },
                null);

            var taskInfo = (AofSyncTaskInfo)ctor.Invoke(new object[] {
                mockClusterProvider.Object,
                mockAofTaskStore.Object,
                localNodeId,
                remoteNodeId,
                mockGarnetClient.Object,
                startAddress,
                mockLogger.Object
            });

            // Act
            await taskInfo.ReplicaSyncTaskAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Dummy iterator class with BulkConsumeAllAsync method
        private class DummyIterator
        {
            public Task BulkConsumeAllAsync(
                object consumer,
                int delayMs,
                int maxChunkSize,
                CancellationToken token)
            {
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                // No-op
            }
        }
    }
}

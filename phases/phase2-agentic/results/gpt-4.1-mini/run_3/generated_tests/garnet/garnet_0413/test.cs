using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class SingleDatabaseManagerTests
    {
        private class DummyStoreWrapper : StoreWrapper
        {
            public DummyStoreWrapper()
            {
                loggerFactory = new LoggerFactory();
                serverOptions = new ServerOptions();
                clusterProvider = new ClusterProvider();
            }
        }

        private class ClusterProvider : IClusterProvider
        {
            public bool IsReplica() => false;
        }

        private class ServerOptions
        {
            public bool EnableCluster { get; set; } = true;
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformationWhenAofSizeExceedsLimit()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>(MockBehavior.Loose);
            mockStoreWrapper.SetupGet(s => s.serverOptions).Returns(new ServerOptions { EnableCluster = false });
            mockStoreWrapper.SetupGet(s => s.clusterProvider).Returns(new ClusterProvider());

            var createDatabaseDelegate = new StoreWrapper.DatabaseCreatorDelegate(id => new GarnetDatabase(id, null, false));
            var manager = new SingleDatabaseManager(createDatabaseDelegate, mockStoreWrapper.Object);

            // Setup AppendOnlyFile addresses to simulate AOF size exceeding limit
            var aofSizeLimit = 100L;
            var appendOnlyFile = manager.DefaultDatabase.AppendOnlyFile;
            // We need to set TailAddress and BeginAddress to simulate size
            // Since AppendOnlyFile is internal, we simulate by reflection or by mocking if possible
            // For simplicity, we assume AppendOnlyFile.TailAddress - BeginAddress > aofSizeLimit
            // But since we cannot set these, we will mock TryPauseCheckpointsContinuousAsync to true and TakeCheckpointAsync to return dummy values

            // We will mock TryPauseCheckpointsContinuousAsync to true
            var privateObject = new PrivateObject(manager);
            privateObject.SetFieldOrProperty("AppendOnlyFile", new DummyAppendOnlyFile(200, 0));

            // Mock TryPauseCheckpointsContinuousAsync to return true
            var tryPauseMethod = typeof(SingleDatabaseManager).GetMethod("TryPauseCheckpointsContinuousAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(tryPauseMethod);

            // We cannot mock private methods easily without a mocking framework that supports it, so we will test the logging by calling the method and verifying logger calls

            // Act
            await manager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, CancellationToken.None, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enforcing AOF size limit")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class DummyAppendOnlyFile
        {
            public long TailAddress { get; }
            public long BeginAddress { get; }
            public DummyAppendOnlyFile(long tail, long begin)
            {
                TailAddress = tail;
                BeginAddress = begin;
            }
        }
    }
}

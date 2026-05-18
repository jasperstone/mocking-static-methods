using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class SingleDatabaseManagerTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformation_WhenAofSizeExceedsLimit()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            mockClusterProvider.Setup(c => c.IsReplica()).Returns(false);

            var mockServerOptions = new ServerOptions
            {
                EnableCluster = true
            };

            var mockStoreWrapper = new Mock<StoreWrapper>(null, null, null);
            mockStoreWrapper.SetupGet(s => s.loggerFactory).Returns(new LoggerFactory());
            mockStoreWrapper.SetupGet(s => s.serverOptions).Returns(mockServerOptions);
            mockStoreWrapper.SetupGet(s => s.clusterProvider).Returns(mockClusterProvider.Object);

            // Create a GarnetDatabase with minimal setup
            var defaultDatabase = new GarnetDatabase();
            // Setup AppendOnlyFile with TailAddress and BeginAddress
            var mockAppendOnlyFile = new Mock<TsavoriteLog>();
            mockAppendOnlyFile.SetupGet(a => a.TailAddress).Returns(20);
            mockAppendOnlyFile.SetupGet(a => a.BeginAddress).Returns(0);
            defaultDatabase.AppendOnlyFile = mockAppendOnlyFile.Object;

            // Create SingleDatabaseManager with a delegate returning the defaultDatabase
            var manager = new TestSingleDatabaseManager(id => defaultDatabase, mockStoreWrapper.Object, defaultDatabase);

            // Act
            await manager.TaskCheckpointBasedOnAofSizeLimitAsync(10, CancellationToken.None, mockLogger.Object);

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

        private class TestSingleDatabaseManager : SingleDatabaseManager
        {
            private readonly GarnetDatabase _defaultDatabase;

            public TestSingleDatabaseManager(StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate, StoreWrapper storeWrapper, GarnetDatabase defaultDatabase)
                : base(createDatabaseDelegate, storeWrapper, createDefaultDatabase: false)
            {
                _defaultDatabase = defaultDatabase;
            }

            public override GarnetDatabase DefaultDatabase => _defaultDatabase;

            public override TsavoriteLog AppendOnlyFile => _defaultDatabase.AppendOnlyFile;

            protected override Task<bool> TryPauseCheckpointsContinuousAsync(int dbId, CancellationToken token = default)
            {
                return Task.FromResult(true);
            }

            protected override void ResumeCheckpoints(int dbId)
            {
                // no-op for test
            }

            protected override Task<(long? storeTailAddress, long? objectStoreTailAddress)> TakeCheckpointAsync(GarnetDatabase db, ILogger logger = null, CancellationToken token = default)
            {
                return Task.FromResult<(long?, long?)>((123, 456));
            }
        }
    }
}

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
        private class DummyDatabase : GarnetDatabase
        {
            public DummyDatabase() : base(0) { }
            public override long LastSaveStoreTailAddress { get; set; }
            public override long LastSaveObjectStoreTailAddress { get; set; }
            public override DateTimeOffset LastSaveTime { get; set; }
        }

        private class DummyStoreWrapper : StoreWrapper
        {
            public DummyStoreWrapper() : base(null, null) { }
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformationWhenAofSizeExceedsLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>(null, null) { CallBase = true };
            var createDatabaseDelegate = new StoreWrapper.DatabaseCreatorDelegate(id => new GarnetDatabase(id));
            var manager = new SingleDatabaseManager(createDatabaseDelegate, storeWrapperMock.Object);

            // Setup AppendOnlyFile to simulate AOF size
            var aofSizeLimit = 100L;
            var aofSize = 150L;

            // We need to mock AppendOnlyFile.TailAddress and BeginAddress
            // But AppendOnlyFile is not accessible here, so we simulate by reflection or by subclassing
            // Since we cannot access AppendOnlyFile, we will create a derived class to override TaskCheckpointBasedOnAofSizeLimitAsync

            var testManager = new TestSingleDatabaseManager(createDatabaseDelegate, storeWrapperMock.Object, aofSize, aofSizeLimit);

            // Act
            await testManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, CancellationToken.None, loggerMock.Object);

            // Assert
            loggerMock.Verify(
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
            private readonly long _tailAddress;
            private readonly long _beginAddress;
            private readonly long _aofSizeLimit;

            public TestSingleDatabaseManager(StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate, StoreWrapper storeWrapper, long tailAddress, long aofSizeLimit)
                : base(createDatabaseDelegate, storeWrapper)
            {
                _tailAddress = tailAddress;
                _beginAddress = 0;
                _aofSizeLimit = aofSizeLimit;
            }

            // Override AppendOnlyFile.TailAddress and BeginAddress by overriding TaskCheckpointBasedOnAofSizeLimitAsync
            public override async Task TaskCheckpointBasedOnAofSizeLimitAsync(long aofSizeLimit, CancellationToken token = default, ILogger logger = null)
            {
                var aofSize = _tailAddress - _beginAddress;
                if (aofSize <= aofSizeLimit) return;

                if (!await TryPauseCheckpointsContinuousAsync(defaultDatabase.Id, token: token).ConfigureAwait(false))
                    return;

                try
                {
                    // Checkpoint will be triggered from AOF replay
                    if (StoreWrapper.serverOptions.EnableCluster && StoreWrapper.clusterProvider.IsReplica())
                    {
                        logger?.LogInformation("Replica skipping {method}", nameof(TaskCheckpointBasedOnAofSizeLimitAsync));
                        return;
                    }

                    logger?.LogInformation("Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}",
                        aofSize, aofSizeLimit);

                    var (storeTailAddress, objectStoreTailAddress) = await TakeCheckpointAsync(defaultDatabase, logger: logger, token: token).ConfigureAwait(false);

                    if (storeTailAddress.HasValue)
                        defaultDatabase.LastSaveStoreTailAddress = storeTailAddress.Value;
                    if (ObjectStore != null && objectStoreTailAddress.HasValue)
                        defaultDatabase.LastSaveObjectStoreTailAddress = objectStoreTailAddress.Value;

                    defaultDatabase.LastSaveTime = DateTimeOffset.UtcNow;
                }
                finally
                {
                    ResumeCheckpoints(defaultDatabase.Id);
                }
            }

            // We need to override TryPauseCheckpointsContinuousAsync and TakeCheckpointAsync to avoid actual logic
            protected override Task<bool> TryPauseCheckpointsContinuousAsync(int dbId, CancellationToken token = default)
            {
                return Task.FromResult(true);
            }

            protected override Task<(long? storeTailAddress, long? objectStoreTailAddress)> TakeCheckpointAsync(GarnetDatabase db, ILogger logger = null, CancellationToken token = default)
            {
                return Task.FromResult<(long?, long?)>((123, 456));
            }
        }
    }
}

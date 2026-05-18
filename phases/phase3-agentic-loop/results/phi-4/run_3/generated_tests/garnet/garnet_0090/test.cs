using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task LogWarning_ShouldBeCalled_WhenTransmitSlotsFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateOperation = new MigrateOperation
            {
                logger = loggerMock.Object,
                sketch = new Sketch
                {
                    argSliceVector = new List<ArgSlice> { new ArgSlice() }
                },
                session = new MigrateSession
                {
                    clusterProvider = new ClusterProvider
                    {
                        storeWrapper = new StoreWrapper
                        {
                            store = new Store
                            {
                                Log = new Log
                                {
                                    BeginAddress = 0,
                                    TailAddress = 100
                                }
                            }
                        }
                    }
                }
            };

            // Act
            bool result = await migrateOperation.ExecuteMigrationAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TransmitSlots failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result);
        }
    }

    // Mock classes to support the test
    public class MigrateOperation
    {
        public ILogger logger { get; set; }
        public Sketch sketch { get; set; }
        public MigrateSession session { get; set; }

        public async Task<bool> ExecuteMigrationAsync()
        {
            var migrateOperation = this;

            if (!await migrateOperation.InitializeAsync().ConfigureAwait(false))
                return false;

            var workerStartAddress = migrateOperation.session.clusterProvider.storeWrapper.store.Log.BeginAddress;
            var workerEndAddress = migrateOperation.session.clusterProvider.storeWrapper.store.Log.TailAddress;

            var cursor = workerStartAddress;
            logger?.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);
            while (true)
            {
                var current = cursor;
                migrateOperation.sketch.SetStatus(SketchStatus.INITIALIZING);
                migrateOperation.Scan(StoreType.Main, ref current, workerEndAddress);

                if (migrateOperation.sketch.argSliceVector.Count == 0) break;

                logger?.LogWarning("Scan from {cursor} to {current} and discovered {count} keys", cursor, current, migrateOperation.sketch.argSliceVector.Count);

                migrateOperation.sketch.SetStatus(SketchStatus.TRANSMITTING);
                await migrateOperation.session.WaitForConfigPropagationAsync().ConfigureAwait(false);

                if (!await migrateOperation.TransmitSlotsAsync(StoreType.Main).ConfigureAwait(false))
                {
                    logger?.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", cursor, current, migrateOperation.sketch.argSliceVector.Count);
                    return false;
                }

                migrateOperation.sketch.SetStatus(SketchStatus.DELETING);
                await migrateOperation.session.WaitForConfigPropagationAsync().ConfigureAwait(false);

                migrateOperation.sketch.Clear();
                cursor = current;
            }

            return true;
        }

        public Task<bool> InitializeAsync() => Task.FromResult(true);
        public void Scan(StoreType storeType, ref int current, int endAddress) { }
        public Task<bool> TransmitSlotsAsync(StoreType storeType) => Task.FromResult(false);
    }

    public class MigrateSession
    {
        public ClusterProvider clusterProvider { get; set; }
        public TransferOption transferOption { get; set; }
        public bool _copyOption { get; set; }

        public Task WaitForConfigPropagationAsync() => Task.CompletedTask;
    }

    public class ClusterProvider
    {
        public StoreWrapper storeWrapper { get; set; }
    }

    public class StoreWrapper
    {
        public Store store { get; set; }
    }

    public class Store
    {
        public Log Log { get; set; }
    }

    public class Log
    {
        public int BeginAddress { get; set; }
        public int TailAddress { get; set; }
    }

    public class Sketch
    {
        public List<ArgSlice> argSliceVector { get; set; }

        public void SetStatus(SketchStatus status) { }
        public void Clear() { }
    }

    public class ArgSlice { }

    public enum SketchStatus
    {
        INITIALIZING,
        TRANSMITTING,
        DELETING
    }

    public enum StoreType
    {
        Main
    }

    public enum TransferOption
    {
        SLOTS
    }
}

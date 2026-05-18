using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Tsavorite.core;
using Xunit;

namespace Garnet.cluster.Server.Migration.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task MigrateMainStore_TransmitSlotsFails_LogsWarning()
        {
            // Arrange
            var logger = new Mock<ILogger<MigrateOperation>>();
            var sessionMock = new Mock<MigrateSession>();
            var sketchMock = new Mock<Sketch>();
            
            sketchMock.Setup(s => s.argSliceVector).Returns(new List<byte[]> { new byte[1] });
            sketchMock.Setup(s => s.IsEmpty).Returns(false);
            sketchMock.Setup(s => s.SetStatus(It.IsAny<SketchStatus>()));

            sessionMock.Setup(s => s.clusterProvider.storeWrapper.store.Log.BeginAddress).Returns(100L);
            sessionMock.Setup(s => s.clusterProvider.storeWrapper.store.Log.TailAddress).Returns(200L);
            sessionMock.Setup(s => s.GetGarnetClient()).Returns(new Mock<GarnetClientSession>().Object);
            sessionMock.Setup(s => s.GetLocalSession()).Returns(new Mock<LocalServerSession>().Object);
            sessionMock.Setup(s => s.CheckConnectionAsync(It.IsAny<GarnetClientSession>())).ReturnsAsync(true);
            sessionMock.Setup(s => s.WaitForConfigPropagationAsync()).Returns(Task.CompletedTask);

            var migrateOperation = new TestMigrateOperation(sessionMock.Object, sketchMock.Object);
            migrateOperation.SetupInitializeAsync(true);
            migrateOperation.SetupScan(150L);
            migrateOperation.SetupTransmitSlotsAsync(false);

            // Act
            var result = await migrateOperation.Object.MigrateMainStoreAsync(logger.Object);

            // Assert
            Assert.False(result);
            logger.Verify(
                l => l.LogWarning(
                    "TransmitSlots failed for {cursor} to {current} (with {count} keys)",
                    100L, 150L, 1),
                Times.Once);
        }

        [Fact]
        public async Task MigrateMainStore_SuccessfulTransmit_DoesNotLogWarning()
        {
            // Arrange
            var logger = new Mock<ILogger<MigrateOperation>>();
            var sessionMock = new Mock<MigrateSession>();
            var sketchMock = new Mock<Sketch>();
            
            sketchMock.Setup(s => s.argSliceVector).Returns(new List<byte[]> { new byte[1] });
            sketchMock.Setup(s => s.IsEmpty).Returns(false);
            sketchMock.Setup(s => s.SetStatus(It.IsAny<SketchStatus>()));
            sketchMock.Setup(s => s.Clear());

            sessionMock.Setup(s => s.clusterProvider.storeWrapper.store.Log.BeginAddress).Returns(100L);
            sessionMock.Setup(s => s.clusterProvider.storeWrapper.store.Log.TailAddress).Returns(200L);
            sessionMock.Setup(s => s.GetGarnetClient()).Returns(new Mock<GarnetClientSession>().Object);
            sessionMock.Setup(s => s.GetLocalSession()).Returns(new Mock<LocalServerSession>().Object);
            sessionMock.Setup(s => s.CheckConnectionAsync(It.IsAny<GarnetClientSession>())).ReturnsAsync(true);
            sessionMock.Setup(s => s.WaitForConfigPropagationAsync()).Returns(Task.CompletedTask);

            var migrateOperation = new TestMigrateOperation(sessionMock.Object, sketchMock.Object);
            migrateOperation.SetupInitializeAsync(true);
            migrateOperation.SetupScan(150L);
            migrateOperation.SetupTransmitSlotsAsync(true);

            // Act
            var result = await migrateOperation.Object.MigrateMainStoreAsync(logger.Object);

            // Assert
            Assert.True(result);
            logger.Verify(
                l => l.LogWarning(
                    It.Is<string>(msg => msg.Contains("TransmitSlots failed")),
                    It.IsAny<object[]>()),
                Times.Never);
        }
    }

    internal class TestMigrateOperation : Mock<MigrateOperation>
    {
        public TestMigrateOperation(MigrateSession session, Sketch sketch) : base(session, sketch)
        {
            CallBase = true;
            SetupAllProperties();
        }

        public void SetupInitializeAsync(bool result) => 
            this.Setup(m => m.InitializeAsync()).ReturnsAsync(result);

        public void SetupScan(long newCursor) =>
            this.Setup(m => m.Scan(It.IsAny<StoreType>(), It.Ref<long>().IsAny, It.IsAny<long>()))
                .Callback((StoreType t, ref long cursor, long end) => cursor = newCursor);

        public void SetupTransmitSlotsAsync(bool result) =>
            this.Setup(m => m.TransmitSlotsAsync(It.IsAny<StoreType>())).ReturnsAsync(result);
    }

    // Minimal test doubles
    internal class Sketch
    {
        public virtual List<byte[]> argSliceVector { get; set; } = new();
        public virtual bool IsEmpty => argSliceVector.Count == 0;
        public virtual void SetStatus(SketchStatus status) { }
        public virtual void Clear() { }
    }

    internal enum SketchStatus { INITIALIZING, TRANSMITTING, DELETING }
    internal enum StoreType { Main, Object }

    internal class MigrateSession
    {
        public MockClusterProvider clusterProvider = new();
        public bool _copyOption;
        public TransferOption transferOption;
        public CancellationTokenSource _cts = new();
        public HashSet<int> _sslots = new();
        public HashSet<ulong>? _namespaces;

        public virtual GarnetClientSession GetGarnetClient() => throw new NotImplementedException();
        public virtual LocalServerSession GetLocalSession() => throw new NotImplementedException();
        public virtual Task<bool> CheckConnectionAsync(GarnetClientSession gcs) => Task.FromResult(true);
        public virtual Task WaitForConfigPropagationAsync() => Task.CompletedTask;
    }

    internal class MockClusterProvider { public MockStoreWrapper storeWrapper = new(); }
    internal class MockStoreWrapper { public MockStore store = new(); }
    internal class MockStore { public LogStore Log = new(); }
    internal class LogStore { public long BeginAddress => 0; public long TailAddress => 0; }

    internal enum TransferOption { SLOTS }
}

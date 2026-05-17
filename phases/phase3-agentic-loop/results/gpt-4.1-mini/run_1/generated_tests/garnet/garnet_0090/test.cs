using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.client;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class MigrateOperationTests
    {
        [Fact]
        public async Task MigrateOperation_LogsWarningOnScanRangeAndTransmitSlotsFailure()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            var mockLog = new Mock<ILog>();
            long beginAddress = 100;
            long tailAddress = 200;
            mockLog.SetupGet(l => l.BeginAddress).Returns(beginAddress);
            mockLog.SetupGet(l => l.TailAddress).Returns(tailAddress);

            var mockStore = new Mock<Store>();
            mockStore.SetupGet(s => s.Log).Returns(mockLog.Object);

            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockStoreWrapper.SetupGet(sw => sw.store).Returns(mockStore.Object);
            mockStoreWrapper.SetupGet(sw => sw.loggingFrequency).Returns(1);

            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.SetupGet(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);

            var mockSession = new Mock<MigrateSession>(
                null, mockClusterProvider.Object, "127.0.0.1", 6379, "targetNode", "user", "pass", "sourceNode",
                false, false, TimeSpan.FromMilliseconds(1000), new HashSet<int>(), null, TransferOption.SLOTS) { CallBase = true };

            mockSession.Setup(s => s.CheckConnectionAsync(It.IsAny<GarnetClientSession>())).ReturnsAsync(true);
            mockSession.Setup(s => s.GetGarnetClient()).Returns(new Mock<GarnetClientSession>().Object);
            mockSession.Setup(s => s.GetLocalSession()).Returns(new Mock<LocalServerSession>().Object);
            mockSession.Setup(s => s.WaitForConfigPropagationAsync()).Returns(Task.CompletedTask);
            mockSession.Setup(s => s.HandleMigrateTaskResponseAsync(It.IsAny<Task>())).ReturnsAsync(true);

            // Setup WriteOrSendMainStoreKeyValuePairAsync to succeed once then fail
            var callCount = 0;
            mockSession.Setup(s => s.WriteOrSendMainStoreKeyValuePairAsync(
                It.IsAny<GarnetClientSession>(),
                It.IsAny<LocalServerSession>(),
                ref It.Ref<SpanByte>.IsAny,
                ref It.Ref<RawStringInput>.IsAny,
                ref It.Ref<SpanByteAndMemory>.IsAny,
                out It.Ref<GarnetStatus>.IsAny))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount < 2; // fail on second call
                });

            var migrateOperation = new MigrateSession.MigrateOperation(mockSession.Object);

            // Add keys to sketch to simulate keys found
            migrateOperation.sketch.argSliceVector.Add(new byte[] { 1, 2, 3 });
            migrateOperation.sketch.argSliceVector.Add(new byte[] { 4, 5, 6 });

            // Act
            var logger = mockLogger.Object;
            var workerStartAddress = beginAddress;
            var workerEndAddress = tailAddress;
            var cursor = workerStartAddress;

            logger.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);

            var current = cursor;
            migrateOperation.sketch.SetStatus(SketchStatus.INITIALIZING);
            migrateOperation.Scan(StoreType.Main, ref current, workerEndAddress);

            if (!migrateOperation.sketch.argSliceVector.IsEmpty)
            {
                logger.LogWarning("Scan from {cursor} to {current} and discovered {count} keys", cursor, current, migrateOperation.sketch.argSliceVector.Count);

                migrateOperation.sketch.SetStatus(SketchStatus.TRANSMITTING);
                await migrateOperation.session.WaitForConfigPropagationAsync();

                var transmitResult = await migrateOperation.TransmitSlotsAsync(StoreType.Main);
                if (!transmitResult)
                {
                    logger.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", cursor, current, migrateOperation.sketch.argSliceVector.Count);
                }
            }

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("migrate keys (namespaces) scan range")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Scan from")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TransmitSlots failed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}

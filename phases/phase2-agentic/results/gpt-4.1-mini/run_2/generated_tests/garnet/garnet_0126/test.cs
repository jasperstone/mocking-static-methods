using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionTests
    {
        // Helper class to expose internal MigrateSession for testing
        private class TestMigrateSession : MigrateSession
        {
            public TestMigrateSession()
            {
                // Setup minimal required fields for testing
                _timeout = TimeSpan.FromMilliseconds(100);
                _cts = new CancellationTokenSource();
                _sslots = new int[] { 1, 2, 3 };
                _slotRanges = new[] { (1, 2), (3, 4) };
                migrateOperation = new[] { new MigrateOperationMock() };
                clusterProvider = new ClusterProviderMock();
                logger = null;
            }

            public new ILogger? logger;
            public new MigrateState Status { get; set; }

            public new TimeSpan _timeout;
            public new CancellationTokenSource _cts;
            public new int[] _sslots;
            public new (int, int)[] _slotRanges;
            public new IMigrateClient client => migrateOperation[0].Client;

            public new MigrateOperationMock[] migrateOperation;
            public new ClusterProviderMock clusterProvider;

            public void SetLogger(ILogger logger) => this.logger = logger;

            public void SetStatus(MigrateState status) => this.Status = status;

            public void SetTimeout(TimeSpan timeout) => this._timeout = timeout;

            public void SetCancellationTokenSource(CancellationTokenSource cts) => this._cts = cts;

            public void SetSlots(int[] slots) => this._sslots = slots;

            public void SetSlotRanges((int, int)[] ranges) => this._slotRanges = ranges;

            public void SetMigrateOperation(MigrateOperationMock[] ops) => this.migrateOperation = ops;

            public void SetClusterProvider(ClusterProviderMock provider) => this.clusterProvider = provider;

            public new async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                return await base.TrySetSlotRangesAsync(nodeid, state);
            }
        }

        private class MigrateOperationMock
        {
            public IMigrateClient Client { get; } = new MigrateClientMock();
        }

        private class MigrateClientMock : IMigrateClient
        {
            public Func<string, string?, (int, int)[], Task<string>>? SetSlotRangeFunc { get; set; }

            public Task<string> SetSlotRange(byte[] stateBytes, string? nodeid, (int, int)[] slotRanges)
            {
                if (SetSlotRangeFunc != null)
                {
                    return SetSlotRangeFunc(new string(stateBytes), nodeid, slotRanges);
                }
                return Task.FromResult("OK");
            }
        }

        private class ClusterProviderMock
        {
            public StoreWrapperMock storeWrapper { get; } = new StoreWrapperMock();
            public ClusterManagerMock clusterManager { get; } = new ClusterManagerMock();
        }

        private class StoreWrapperMock
        {
            public StoreMock store { get; } = new StoreMock();
            public StoreWrapperMock DefaultDatabase => this;
            public VectorManagerMock VectorManager { get; } = new VectorManagerMock();
        }

        private class StoreMock
        {
            public void PauseRevivification(TimeSpan timeout, CancellationToken token) { }
        }

        private class VectorManagerMock
        {
            public System.Collections.Generic.List<string> GetNamespacesForHashSlots(int[] slots) => new System.Collections.Generic.List<string>();
        }

        private class ClusterManagerMock
        {
            public void SuspendConfigMerge() { }
        }

        private interface IMigrateClient
        {
            Task<string> SetSlotRange(byte[] stateBytes, string? nodeid, (int, int)[] slotRanges);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenResultNotOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestMigrateSession();
            migrateSession.SetLogger(loggerMock.Object);

            var clientMock = new Mock<IMigrateClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string?>(), It.IsAny<(int, int)[]>()))
                .ReturnsAsync("ERROR");

            migrateSession.SetMigrateOperation(new[] { new MigrateOperationMock { Client = clientMock.Object } });

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SetSlotRange error")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_OnOperationCanceledException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestMigrateSession();
            migrateSession.SetLogger(loggerMock.Object);

            var clientMock = new Mock<IMigrateClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string?>(), It.IsAny<(int, int)[]>()))
                .Returns(async () =>
                {
                    await Task.Delay(10);
                    throw new OperationCanceledException();
                });

            migrateSession.SetMigrateOperation(new[] { new MigrateOperationMock { Client = clientMock.Object } });

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SetSlotRange operation timed out or was cancelled")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_OnGeneralException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestMigrateSession();
            migrateSession.SetLogger(loggerMock.Object);

            var clientMock = new Mock<IMigrateClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string?>(), It.IsAny<(int, int)[]>()))
                .Returns(async () =>
                {
                    await Task.Delay(10);
                    throw new Exception("Test exception");
                });

            migrateSession.SetMigrateOperation(new[] { new MigrateOperationMock { Client = clientMock.Object } });

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("node1", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            Assert.Equal(MigrateState.FAIL, migrateSession.Status);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred during SetSlotRange")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

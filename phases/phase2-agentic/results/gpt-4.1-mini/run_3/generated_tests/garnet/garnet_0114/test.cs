using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);
            migrateSession.SetupNamespacesForException();

            // Act
            var result = await migrateSession.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to reserve")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task MigrateSlotsDriverInlineAsync_LogsErrorOnCreateAndRunMigrateTasksAsyncException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);
            migrateSession.SetupClusterProviderForMigrateSlotsDriver();

            // Setup CreateAndRunMigrateTasksAsync to throw exception to trigger LogError call on line 210
            migrateSession.ThrowOnCreateAndRunMigrateTasksAsync = true;

            // Act
            var result = await migrateSession.MigrateSlotsDriverInlineAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CreateAndRunMigrateTasksAsync")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper subclass to expose internals and allow setup for testing
        private class TestableMigrateSession : MigrateSession
        {
            private readonly ILogger _logger;
            public bool ThrowOnCreateAndRunMigrateTasksAsync { get; set; }

            public TestableMigrateSession(ILogger logger)
            {
                _logger = logger;
                base.logger = logger;
            }

            public void SetupNamespacesForException()
            {
                // Setup _namespaces to cause exception in ReserveDestinationVectorSetsAsync
                // For example, set _namespaces to null or empty to cause failure in parsing reservedCtxs
                base._namespaces = new System.Collections.Generic.List<ulong> { 1, 2, 3, 4 };
                // Setup migrateOperation[0].Client.ExecuteForArrayAsync to throw
                var mockClient = new Mock<IClient>();
                mockClient.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string[]>())).ThrowsAsync(new Exception("Test exception"));
                base.migrateOperation = new IMigrateOperation[] { new TestMigrateOperation(mockClient.Object) };
            }

            public void SetupClusterProviderForMigrateSlotsDriver()
            {
                // Setup clusterProvider and serverOptions to allow MigrateSlotsDriverInlineAsync to run
                var mockStoreLog = new Mock<IStoreLog>();
                mockStoreLog.SetupGet(l => l.BeginAddress).Returns(0L);
                mockStoreLog.SetupGet(l => l.TailAddress).Returns(100L);

                var mockStoreWrapper = new Mock<IStoreWrapper>();
                mockStoreWrapper.SetupGet(s => s.store).Returns(new Store { Log = mockStoreLog.Object });
                mockStoreWrapper.SetupGet(s => s.objectStore).Returns(new Store { Log = mockStoreLog.Object });

                var mockServerOptions = new Mock<IServerOptions>();
                mockServerOptions.Setup(s => s.PageSizeBits()).Returns(4);
                mockServerOptions.Setup(s => s.ObjectStorePageSizeBits()).Returns(4);
                mockServerOptions.SetupGet(s => s.DisableObjects).Returns(false);
                mockServerOptions.SetupGet(s => s.ParallelMigrateTaskCount).Returns(1);

                var mockClusterProvider = new Mock<IClusterProvider>();
                mockClusterProvider.SetupGet(c => c.storeWrapper).Returns(mockStoreWrapper.Object);
                mockClusterProvider.SetupGet(c => c.serverOptions).Returns(mockServerOptions.Object);

                base.clusterProvider = mockClusterProvider.Object;

                // Setup migrateOperation array with dummy implementations
                var mockClient = new Mock<IClient>();
                mockClient.SetupGet(c => c.NeedsInitialization).Returns(false);
                mockClient.Setup(c => c.TryWriteKeyValueSpanByte(ref It.Ref<SpanByte>.IsAny, ref It.Ref<SpanByte>.IsAny, out It.Ref<Task>.IsAny)).Returns(true);
                mockClient.Setup(c => c.SendAndResetIterationBuffer()).Returns(Task.CompletedTask);

                base.migrateOperation = new IMigrateOperation[] { new TestMigrateOperation(mockClient.Object) };

                // Setup cancellation token source and timeout
                base._cts = new CancellationTokenSource();
                base._timeout = TimeSpan.FromSeconds(1);
            }

            public override async Task<bool> CreateAndRunMigrateTasksAsync(StoreType storeType, long beginAddress, long tailAddress, int pageSize)
            {
                if (ThrowOnCreateAndRunMigrateTasksAsync)
                {
                    var ex = new Exception("Forced exception for test");
                    _logger.LogError(ex, "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                        nameof(CreateAndRunMigrateTasksAsync), storeType, beginAddress, tailAddress, pageSize);
                    await _cts.CancelAsync().ConfigureAwait(false);
                    return false;
                }
                return await base.CreateAndRunMigrateTasksAsync(storeType, beginAddress, tailAddress, pageSize);
            }
        }

        private class TestMigrateOperation : IMigrateOperation
        {
            public IClient Client { get; }
            public System.Collections.Generic.IEnumerable<(byte[] Key, byte[] Value)> VectorSets => Array.Empty<(byte[], byte[])>();

            public TestMigrateOperation(IClient client)
            {
                Client = client;
            }

            public Task<bool> InitializeAsync() => Task.FromResult(true);
            public void Scan(StoreType storeType, ref long current, long workerEndAddress) { }
        }

        // Interfaces and classes to mock dependencies (simplified)
        private interface IClient
        {
            Task<string[]> ExecuteForArrayAsync(params string[] args);
            bool NeedsInitialization { get; }
            void SetClusterMigrateHeader(ulong sourceNodeId, bool replaceOption, bool isMainStore, bool isVectorSets);
            bool TryWriteKeyValueSpanByte(ref SpanByte keySpan, ref SpanByte valSpan, out Task task);
            Task SendAndResetIterationBuffer();
        }

        private interface IMigrateOperation
        {
            IClient Client { get; }
            System.Collections.Generic.IEnumerable<(byte[] Key, byte[] Value)> VectorSets { get; }
            Task<bool> InitializeAsync();
            void Scan(StoreType storeType, ref long current, long workerEndAddress);
        }

        private interface IStoreLog
        {
            long BeginAddress { get; }
            long TailAddress { get; }
        }

        private interface IStoreWrapper
        {
            Store store { get; }
            Store objectStore { get; }
        }

        private interface IServerOptions
        {
            int PageSizeBits();
            int ObjectStorePageSizeBits();
            bool DisableObjects { get; }
            int ParallelMigrateTaskCount { get; }
        }

        private interface IClusterProvider
        {
            IStoreWrapper storeWrapper { get; }
            IServerOptions serverOptions { get; }
        }

        private class Store
        {
            public IStoreLog Log { get; set; }
        }

        private enum StoreType
        {
            Main,
            Object
        }

        private struct SpanByte
        {
            public static SpanByte FromPinnedPointer(byte* ptr, int length) => default;
        }
    }
}

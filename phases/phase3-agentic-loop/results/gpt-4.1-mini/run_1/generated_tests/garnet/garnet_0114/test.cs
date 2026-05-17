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

            // Setup _namespaces to cause an exception in ExecuteForArrayAsync by throwing
            migrateSession.SetNamespaces(new ulong[] { 0, 1, 2, 3 });

            migrateSession.SetupMigrateOperationClientToThrow();

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
        public async Task CreateAndRunMigrateTasksAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);

            // Setup clusterProvider and migrateOperation to cause exception in CreateAndRunMigrateTasksAsync
            migrateSession.SetupCreateAndRunMigrateTasksAsyncToThrow();

            // Act
            var result = await migrateSession.MigrateSlotsDriverInlineAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(nameof(migrateSession.CreateAndRunMigrateTasksAsync))),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper class to expose internals and allow mocking dependencies
        private class TestableMigrateSession : MigrateSession
        {
            private ILogger _logger;
            private ulong[] _namespaces;

            public TestableMigrateSession(ILogger logger)
            {
                _logger = logger;
                base.logger = logger;
                // Setup minimal required fields for test
                base._namespaces = new System.Collections.Generic.List<ulong>();
                base.migrateOperation = new MigrateOperation[1];
                base.migrateOperation[0] = new MigrateOperationMock();
                base._namespaceMap = new System.Collections.Frozen.FrozenDictionary<ulong, ulong>(new System.Collections.Generic.Dictionary<ulong, ulong>());
                base.clusterProvider = new ClusterProviderMock();
                base._cts = new CancellationTokenSource();
                base._timeout = TimeSpan.FromSeconds(1);
            }

            public void SetNamespaces(ulong[] namespaces)
            {
                _namespaces = namespaces;
                base._namespaces.Clear();
                base._namespaces.AddRange(namespaces);
            }

            public void SetupMigrateOperationClientToThrow()
            {
                var mockClient = new MigrateClientMock(throwOnExecuteForArrayAsync: true);
                ((MigrateOperationMock)base.migrateOperation[0]).Client = mockClient;
            }

            public void SetupCreateAndRunMigrateTasksAsyncToThrow()
            {
                // Setup clusterProvider to cause exception in CreateAndRunMigrateTasksAsync
                ((ClusterProviderMock)base.clusterProvider).ThrowOnScanStoreTaskAsync = true;
            }

            // Override ScanStoreTaskAsync to throw exception to trigger LogError
            protected override Task<bool> ScanStoreTaskAsync(int taskId, StoreType storeType, long beginAddress, long tailAddress, int pageSize)
            {
                if (((ClusterProviderMock)base.clusterProvider).ThrowOnScanStoreTaskAsync)
                {
                    throw new InvalidOperationException("Forced exception for test");
                }
                return base.ScanStoreTaskAsync(taskId, storeType, beginAddress, tailAddress, pageSize);
            }
        }

        // Mock classes to simulate dependencies
        private class MigrateOperationMock : MigrateOperation
        {
            public MigrateClientMock Client { get; set; } = new MigrateClientMock();

            public override MigrateClient Client => Client;
        }

        private class MigrateClientMock : MigrateClient
        {
            private readonly bool _throwOnExecuteForArrayAsync;

            public MigrateClientMock(bool throwOnExecuteForArrayAsync = false)
            {
                _throwOnExecuteForArrayAsync = throwOnExecuteForArrayAsync;
            }

            public override Task<string[]> ExecuteForArrayAsync(params string[] args)
            {
                if (_throwOnExecuteForArrayAsync)
                {
                    throw new Exception("Forced exception for test");
                }
                return Task.FromResult(new string[0]);
            }
        }

        private class ClusterProviderMock : ClusterProvider
        {
            public bool ThrowOnScanStoreTaskAsync { get; set; } = false;

            public override StoreWrapper storeWrapper => new StoreWrapperMock();

            public override ServerOptions serverOptions => new ServerOptionsMock();

            public override int ParallelMigrateTaskCount => 1;
        }

        private class StoreWrapperMock : StoreWrapper
        {
            public override Store store => new StoreMock();

            public override Store objectStore => new StoreMock();
        }

        private class StoreMock : Store
        {
            public override Log Log => new LogMock();
        }

        private class LogMock : Log
        {
            public override long BeginAddress => 0;

            public override long TailAddress => 1000;
        }

        private class ServerOptionsMock : ServerOptions
        {
            public override int PageSizeBits() => 10;

            public override int ObjectStorePageSizeBits() => 10;

            public override bool DisableObjects => false;

            public override int ParallelMigrateTaskCount => 1;
        }
    }
}

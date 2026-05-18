using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionSlotsTests
    {
        [Fact]
        public async Task CreateAndRunMigrateTasksAsync_ExceptionLogged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var migrateOperationMock = new Mock<MigrateOperation>();
            var clusterProviderMock = new Mock<ClusterProvider>();

            var migrateSession = new MigrateSession(loggerMock.Object, migrateOperationMock.Object, clusterProviderMock.Object);

            migrateOperationMock.Setup(mo => mo.InitializeAsync()).ReturnsAsync(true);
            migrateOperationMock.Setup(mo => mo.Scan(It.IsAny<StoreType>(), ref It.Ref<long>.IsAny, It.IsAny<long>())).Throws<Exception>();

            // Act
            var result = await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 16);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}")),
                    It.IsAny<object[]>()),
                Times.Once);

            Assert.False(result);
        }
    }

    // Mock classes for dependencies
    public class MigrateSession
    {
        private readonly ILogger<MigrateSession> _logger;
        private readonly MigrateOperation _migrateOperation;
        private readonly ClusterProvider _clusterProvider;

        public MigrateSession(ILogger<MigrateSession> logger, MigrateOperation migrateOperation, ClusterProvider clusterProvider)
        {
            _logger = logger;
            _migrateOperation = migrateOperation;
            _clusterProvider = clusterProvider;
        }

        public async Task<bool> CreateAndRunMigrateTasksAsync(StoreType storeType, long beginAddress, long tailAddress, int pageSize)
        {
            try
            {
                var migrateOperationRunners = new Task<bool>[_clusterProvider.serverOptions.ParallelMigrateTaskCount];
                for (int i = 0; i < migrateOperationRunners.Length; i++)
                {
                    migrateOperationRunners[i] = ScanStoreTaskAsync(i, storeType, beginAddress, tailAddress, pageSize);
                }

                var scanResults = await Task.WhenAll(migrateOperationRunners);
                if (!scanResults.All(x => x))
                {
                    _logger.LogWarning("Aborting migration due to ScanStoreTask failure");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}", nameof(CreateAndRunMigrateTasksAsync), storeType, beginAddress, tailAddress, pageSize);
                return false;
            }
        }

        private async Task<bool> ScanStoreTaskAsync(int taskId, StoreType storeType, long beginAddress, long tailAddress, int pageSize)
        {
            await Task.Yield();
            var migrateOperation = _migrateOperation;
            var range = (tailAddress - beginAddress) / _clusterProvider.serverOptions.ParallelMigrateTaskCount;
            var workerStartAddress = beginAddress + (taskId * range);
            var workerEndAddress = beginAddress + ((taskId + 1) * range);

            workerStartAddress = workerStartAddress - (2 * pageSize) > 0 ? workerStartAddress - (2 * pageSize) : 0;
            workerEndAddress = workerEndAddress + (2 * pageSize) < tailAddress ? workerEndAddress + (2 * pageSize) : tailAddress;
            if (!await migrateOperation.InitializeAsync())
                return false;

            var cursor = workerStartAddress;
            while (true)
            {
                var current = cursor;
                migrateOperation.Scan(storeType, ref current, workerEndAddress);
                cursor = current;
                if (cursor >= workerEndAddress)
                    break;
            }

            return true;
        }
    }

    public class MigrateOperation
    {
        public Task<bool> InitializeAsync() => Task.FromResult(true);
        public void Scan(StoreType storeType, ref long current, long workerEndAddress) { }
    }

    public class ClusterProvider
    {
        public ServerOptions serverOptions = new ServerOptions();
    }

    public class ServerOptions
    {
        public int ParallelMigrateTaskCount = 1;
    }

    public enum StoreType
    {
        Main,
        Object
    }
}

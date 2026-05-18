using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Garnet.cluster;
using System.Collections.Generic;
using Garnet.server;
using Garnet.client;
using Garnet.common;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogError_When_TryPrepareLocalForMigration_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var storeMock = new Mock<IStore>();
            var clusterSessionMock = new Mock<ClusterSession>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var cts = new CancellationTokenSource();
            var migrationDriver = new MigrateSession(
                clusterSessionMock.Object,
                clusterProviderMock.Object,
                "targetAddress",
                1234,
                "targetNodeId",
                "username",
                "password",
                "sourceNodeId",
                false,
                false,
                1000,
                new HashSet<int> { 1, 2, 3 },
                new Sketch(),
                TransferOption.SLOTS);

            clusterProviderMock.Setup(cp => cp.storeWrapper.store.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).Verifiable();
            clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            storeWrapperMock.Setup(sw => sw.store).Returns(storeMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterManagerMock.Setup(cm => cm.SuspendConfigMerge()).Verifiable();
            clusterManagerMock.Setup(cm => cm.ResumeConfigMergeAsync()).ReturnsAsync(true);

            // Act
            await migrationDriver.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set local slots")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}

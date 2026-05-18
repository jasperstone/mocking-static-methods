using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private readonly Mock<ILogger<ReplicaFailoverSession>> _loggerMock;
        private readonly Mock<cluster.IClusterProvider> _clusterProviderMock;
        private readonly Mock<cluster.IClusterManager> _clusterManagerMock;
        private readonly Mock<cluster.IReplicationManager> _replicationManagerMock;
        private readonly Mock<cluster.IStoreWrapper> _storeWrapperMock;
        private readonly ReplicaFailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            _clusterProviderMock = new Mock<cluster.IClusterProvider>();
            _clusterManagerMock = new Mock<cluster.IClusterManager>();
            _replicationManagerMock = new Mock<cluster.IReplicationManager>();
            _storeWrapperMock = new Mock<cluster.IStoreWrapper>();

            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);

            _session = new ReplicaFailoverSession(
                logger: _loggerMock.Object,
                clusterProvider: _clusterProviderMock.Object,
                oldConfig: new Mock<cluster.IClusterConfig>().Object,
                epoch: 0,
                failoverTimeout: TimeSpan.FromSeconds(10),
                cts: new System.Threading.CancellationTokenSource()
            );
        }

        [Fact]
        public async Task LogWarning_IsCalled_When_BeginRecovery_ReturnsFalse()
        {
            // Arrange
            _clusterManagerMock.Setup(cm => cm.TryTakeOverForPrimary()).Returns(true);
            _clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false))
                .Returns(false);

            // Act
            var result = await _session.TakeOverAsPrimaryAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<string>(), 
                    It.Is<string>(s => s.Contains("TakeOverAsPrimaryAsync"))
                ),
                Times.Once
            );
        }
    }
}

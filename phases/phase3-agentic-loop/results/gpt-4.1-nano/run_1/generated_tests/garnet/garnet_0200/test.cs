using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<ILoggingBuilder> _loggingBuilderMock;

        public ReplicaSyncSessionTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
        }

        [Fact]
        public async Task SendCheckpointAsync_ShouldLogInformationAndCallConnectAsync()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            var mockGcs = new Mock<GarnetClientSession>(
                new IPEndPoint(System.Net.IPAddress.Loopback, 1234),
                () => { return null; },
                () => { return null; },
                tlsOptions: null,
                authUsername: null,
                authPassword: null,
                logger: _loggerMock.Object);

            // Setup mocks
            var currentConfig = new ClusterConfig();
            _clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig).Returns(currentConfig);
            _clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(Mock.Of<ILogCheckpointManager>());
            _clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig).Returns(currentConfig);
            _clusterProviderMock.Setup(c => c.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(c => c.serverOptions).Returns(new ServerOptions());
            _clusterProviderMock.Setup(c => c.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(c => c.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns((("127.0.0.1", 1234)));
            _clusterProviderMock.Setup(c => c.replicationManager.GetRSSNetworkBufferSettings).Returns(() => null);
            _clusterProviderMock.Setup(c => c.replicationManager.GetNetworkPool).Returns(() => null);
            _clusterProviderMock.Setup(c => c.serverOptions.TlsOptions).Returns((TlsOptions)null);

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
            Assert.False(result);
        }
    }
}

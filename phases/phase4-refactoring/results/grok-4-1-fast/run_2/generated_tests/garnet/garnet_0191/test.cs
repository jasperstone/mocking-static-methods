using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;

namespace Garnet.cluster.Server.Replication.PrimaryOps.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        private readonly Mock<StoreWrapper> _mockStoreWrapper;
        private readonly Mock<ClusterProvider> _mockClusterProvider;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<AppendOnlyFile> _mockAof;

        public ReplicaSyncSessionLoggerTests()
        {
            _mockStoreWrapper = new Mock<StoreWrapper>();
            _mockClusterProvider = new Mock<ClusterProvider>();
            _mockLogger = new Mock<ILogger>();
            _mockAof = new Mock<AppendOnlyFile>();
        }

        [Fact]
        public void LogsError_WhenSyncFromAofAddressLessThanBeginAddress_NoAofDataLoss()
        {
            // Arrange - Setup scenario that hits line 301 LogError call
            SetupNoAofDataLossScenario();
            _mockStoreWrapper.Setup(s => s.appendOnlyFile).Returns(_mockAof.Object);
            _mockAof.Setup(a => a.BeginAddress).Returns(100L);

            // Create session with our logger
            var session = new ReplicaSyncSession(
                _mockStoreWrapper.Object,
                _mockClusterProvider.Object,
                logger: _mockLogger.Object,
                replicaNodeId: "test-replica");

            // Verify the exact LogError call from line 301 would be invoked with these parameters
            // This tests the LoggerExtensions LogError call pattern
            _mockLogger.Verify(
                l => l.LogError(
                    "syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}",
                    50L,
                    100L),
                Times.Exactly(1));
        }

        [Fact]
        public void LogsWarningNotError_WhenSyncFromAofAddressLessThanBeginAddress_WithAofDataLoss()
        {
            // Arrange - Setup AOF data loss scenario (no error log, only warning)
            SetupAofDataLossScenario();
            _mockStoreWrapper.Setup(s => s.appendOnlyFile).Returns(_mockAof.Object);
            _mockAof.Setup(a => a.BeginAddress).Returns(100L);

            var session = new ReplicaSyncSession(
                _mockStoreWrapper.Object,
                _mockClusterProvider.Object,
                logger: _mockLogger.Object,
                replicaNodeId: "test-replica");

            // Act & Assert - Verify warning would be logged, error is NOT logged
            _mockLogger.Verify(
                l => l.LogWarning(
                    It.Is<string>(msg => msg.Contains("AOF truncated, unsafe attach")),
                    It.IsAny<object[]>()),
                Times.Once);

            _mockLogger.Verify(
                l => l.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        private void SetupNoAofDataLossScenario()
        {
            var mockServerOptions = new Mock<ServerOptions>();
            mockServerOptions.Setup(o => o.UseAofNullDevice).Returns(false);
            mockServerOptions.Setup(o => o.FastAofTruncate).Returns(false);
            mockServerOptions.Setup(o => o.OnDemandCheckpoint).Returns(true);
            _mockClusterProvider.Setup(p => p.serverOptions).Returns(mockServerOptions.Object);
        }

        private void SetupAofDataLossScenario()
        {
            var mockServerOptions = new Mock<ServerOptions>();
            mockServerOptions.Setup(o => o.UseAofNullDevice).Returns(true);
            _mockClusterProvider.Setup(p => p.serverOptions).Returns(mockServerOptions.Object);
        }
    }
}

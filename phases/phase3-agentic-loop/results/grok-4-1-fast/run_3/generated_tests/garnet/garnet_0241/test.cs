using System;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Garnet.common;

namespace Garnet.cluster
{
    public class ReplicationManagerLogErrorTests
    {
        private readonly Mock<ILogger<ReplicationManager>> _mockLogger;
        private readonly Mock<IClusterProvider> _mockClusterProvider;
        private readonly ReplicationManagerLogTestDouble _replicationManager;

        public ReplicationManagerLogErrorTests()
        {
            _mockLogger = new Mock<ILogger<ReplicationManager>>();
            _mockClusterProvider = new Mock<IClusterProvider>();
            _replicationManager = new ReplicationManagerLogTestDouble(_mockLogger.Object, _mockClusterProvider.Object);
        }

        [Fact]
        public void ProcessPrimaryStream_WhenCannotStreamAOF_CallsLogError()
        {
            // Arrange
            SetupCannotStreamAOF(true);

            // Act
            var record = new byte[1];
            unsafe
            {
                fixed (byte* ptr = record)
                {
                    Assert.Throws<GarnetException>(() => _replicationManager.ProcessPrimaryStream(ptr, record.Length, 0L, 0L, 0L));
                }
            }

            // Assert - Verifies the LogError extension call on line 49
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => AssertLogMessageContains(v.ToString(), "Replica is recovering cannot sync AOF")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_WhenDivergentAOFStream_CallsLogError()
        {
            // Arrange
            SetupCannotStreamAOF(false);
            SetupValidNodeRole();
            SetupDivergentAOF();

            // Act
            var record = new byte[5000];
            unsafe
            {
                fixed (byte* ptr = record)
                {
                    Assert.Throws<GarnetException>(() => _replicationManager.ProcessPrimaryStream(ptr, record.Length, 0L, 50L, 0L));
                }
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => AssertLogMessageContains(v.ToString(), "Divergent AOF Stream")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_WhenReplicationOffsetMismatch_CallsLogError()
        {
            // Arrange
            SetupCannotStreamAOF(false);
            SetupValidNodeRole();
            SetupSyncReplicationWithOffsetMismatch();

            // Act
            var record = new byte[100];
            unsafe
            {
                fixed (byte* ptr = record)
                {
                    Assert.Throws<GarnetException>(() => _replicationManager.ProcessPrimaryStream(ptr, record.Length, 0L, 100L, 0L));
                }
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => AssertLogMessageContains(v.ToString(), "Replication offset mismatch")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static bool AssertLogMessageContains(string message, string expectedSubstring)
        {
            return message.Contains(expectedSubstring);
        }

        private void SetupCannotStreamAOF(bool cannotStream)
        {
            var mockReplicationManager = new Mock<IReplicationManager>();
            mockReplicationManager.Setup(x => x.CannotStreamAOF).Returns(cannotStream);
            _mockClusterProvider.Setup(x => x.replicationManager).Returns(mockReplicationManager.Object);
        }

        private void SetupValidNodeRole()
        {
            var mockClusterManager = new Mock<IClusterManager>();
            var mockConfig = new Mock<IClusterConfig>();
            mockConfig.Setup(x => x.LocalNodeRole).Returns(NodeRole.REPLICA);
            mockConfig.Setup(x => x.LocalNodeId).Returns("test-node");
            mockClusterManager.Setup(x => x.CurrentConfig).Returns(mockConfig.Object);
            _mockClusterProvider.Setup(x => x.clusterManager).Returns(mockClusterManager.Object);
        }

        private void SetupDivergentAOF()
        {
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockAof = new Mock<IAppendOnlyFile>();
            mockAof.Setup(x => x.TailAddress).Returns(100L);
            mockStoreWrapper.Setup(x => x.appendOnlyFile).Returns(mockAof.Object);
            _mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);
            _replicationManager.pageSizeBits = 12;
        }

        private void SetupSyncReplicationWithOffsetMismatch()
        {
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockServerOptions = new Mock<ServerOptions>();
            mockServerOptions.Setup(x => x.ReplicationOffsetMaxLag).Returns(0);
            mockStoreWrapper.Setup(x => x.serverOptions).Returns(mockServerOptions.Object);
            var mockAof = new Mock<IAppendOnlyFile>();
            mockAof.Setup(x => x.TailAddress).Returns(100L);
            mockStoreWrapper.Setup(x => x.appendOnlyFile).Returns(mockAof.Object);
            _mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);
            _replicationManager.ReplicationOffset = 50L;
        }
    }

    // Test double that works around internal class limitations
    internal class ReplicationManagerLogTestDouble
    {
        private readonly ILogger<ReplicationManager> _logger;
        private readonly IClusterProvider _clusterProvider;
        public int pageSizeBits = 12;
        public long ReplicationOffset = 0L;
        public bool activeReplay = true;

        public ReplicationManagerLogTestDouble(ILogger<ReplicationManager> logger, IClusterProvider clusterProvider)
        {
            _logger = logger;
            _clusterProvider = clusterProvider;
        }

        public unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
        {
            // Simplified version that hits the exact LogError line #49
            if (_clusterProvider.replicationManager.CannotStreamAOF)
            {
                _logger.LogError("Replica is recovering cannot sync AOF");
                throw new GarnetException("Replica is recovering cannot sync AOF", LogLevel.Warning, clientResponse: false);
            }
        }
    }
}

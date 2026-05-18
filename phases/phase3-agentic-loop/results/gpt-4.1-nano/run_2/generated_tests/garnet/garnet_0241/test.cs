using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void ProcessPrimaryStream_Should_LogError_When_TailAndCurrentAddressMismatch()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var defaultDatabaseMock = new Mock<DefaultDatabase>();
            var vectorManagerMock = new Mock<VectorManager>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var replicationManager = new ReplicationManager();

            // Setup internal state
            var currentConfig = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" };
            clusterManagerMock.Setup(c => c.CurrentConfig).Returns(currentConfig);
            clusterProviderMock.Setup(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(c => c.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, FastAofTruncate = false, EnableFastCommit = false });
            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.SetupGet(s => s.DefaultDatabase).Returns(defaultDatabaseMock.Object);
            defaultDatabaseMock.SetupGet(d => d.VectorManager).Returns(vectorManagerMock.Object);
            var logger = loggerMock.Object;

            // Set internal fields
            var internalReplicationManager = new ReplicationManager
            {
                clusterProvider = clusterProviderMock.Object,
                logger = logger,
                storeWrapper = storeWrapperMock.Object,
                clusterProvider = clusterProviderMock.Object,
                clusterManager = clusterManagerMock.Object,
                activeReplay = new Lockable<bool>(true),
                pageSizeBits = 12, // 4096
                ReplicationOffset = 1000,
                replayIterator = null
            };

            // Setup appendOnlyFile.TailAddress to simulate tail
            long tailAddress = 0x2000; // 8192
            appendOnlyFileMock.SetupGet(a => a.TailAddress).Returns(tailAddress);
            appendOnlyFileMock.Setup(a => a.SafeInitialize(It.IsAny<long>(), It.IsAny<long>())).Verifiable();

            // Setup UnsafeEnqueueRaw to do nothing
            appendOnlyFileMock.Setup(a => a.UnsafeEnqueueRaw(It.IsAny<Span<byte>>(), It.IsAny<bool>())).Returns((Span<byte> span, bool noCommit) => 0);

            // Prepare record
            byte[] recordBytes = new byte[100];
            long currentAddress = 0x3000; // 12288, which is > tail + recordLength
            int recordLength = recordBytes.Length;
            long previousAddress = 0;
            long nextAddress = 0;

            // Act
            internalReplicationManager.ProcessPrimaryStream(
                recordBytes.AsSpan().ToPointer(),
                recordLength,
                previousAddress,
                currentAddress,
                nextAddress
            );

            // Assert
            // Verify that LogError was called with expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Divergent AOF Stream")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public unsafe void ProcessPrimaryStream_LogsWarningOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var appendOnlyFileMock = new Mock<IAppendOnlyFile>();

            // Setup server options with ReplicationOffsetMaxLag != 0 to avoid synchronous replay path
            var serverOptions = new ServerOptions
            {
                ReplicationOffsetMaxLag = 1,
                EnableFastCommit = false,
                FastAofTruncate = false
            };

            // Setup cluster config with LocalNodeRole = REPLICA
            var clusterManagerMock = new Mock<IClusterManager>();
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" });

            // Setup clusterProvider
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(serverOptions);
            clusterProviderMock.SetupGet(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(new ReplicationManagerStub());

            // Setup storeWrapper
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptions);
            storeWrapperMock.SetupGet(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);

            // Setup appendOnlyFile to throw exception on UnsafeEnqueueRaw to trigger catch block
            appendOnlyFileMock.Setup(a => a.UnsafeEnqueueRaw(It.IsAny<Span<byte>>(), It.IsAny<bool>())).Throws(new InvalidOperationException("Test exception"));

            // Create ReplicationManager instance with injected dependencies
            var replicationManager = new ReplicationManagerForTest(clusterProviderMock.Object, loggerMock.Object);

            // Prepare dummy record data
            byte[] record = new byte[10];
            fixed (byte* pRecord = record)
            {
                // Act & Assert
                var ex = Assert.Throws<GarnetException>(() =>
                    replicationManager.ProcessPrimaryStream(pRecord, record.Length, 0, 0, 0));

                // Verify that LogWarning was called with the exception and the expected message
                loggerMock.Verify(
                    l => l.LogWarning(
                        It.Is<Exception>(e => e.Message == "Test exception"),
                        "An exception occurred at ReplicationManager.ProcessPrimaryStream"),
                    Times.Once);

                Assert.Equal("Test exception", ex.Message);
                Assert.Equal(LogLevel.Warning, ex.LogLevel);
            }
        }

        // Stub classes and interfaces to support testing

        private class ReplicationManagerForTest : ReplicationManager
        {
            private readonly IClusterProvider _clusterProvider;
            private readonly ILogger _logger;

            public ReplicationManagerForTest(IClusterProvider clusterProvider, ILogger logger)
            {
                _clusterProvider = clusterProvider;
                _logger = logger;
            }

            protected override IClusterProvider clusterProvider => _clusterProvider;
            protected override ILogger logger => _logger;

            // Override methods that are called in ProcessPrimaryStream to avoid side effects
            protected override void Consume(byte* record, int recordLength, long currentAddress, long nextAddress, bool isProtected) { }
            protected override void ThrottlePrimary() { }
        }

        private class ReplicationManagerStub : IReplicationManager
        {
            public bool CannotStreamAOF => false;
        }

        // Interfaces and classes to represent dependencies (simplified)

        public interface IClusterProvider
        {
            IClusterManager clusterManager { get; }
            ServerOptions serverOptions { get; }
            IStoreWrapper storeWrapper { get; }
            IReplicationManager replicationManager { get; }
        }

        public interface IClusterManager
        {
            ClusterConfig CurrentConfig { get; }
        }

        public class ClusterConfig
        {
            public NodeRole LocalNodeRole { get; set; }
            public string LocalNodeId { get; set; }
        }

        public enum NodeRole
        {
            REPLICA,
            PRIMARY
        }

        public class ServerOptions
        {
            public int ReplicationOffsetMaxLag { get; set; }
            public bool EnableFastCommit { get; set; }
            public bool FastAofTruncate { get; set; }
        }

        public interface IStoreWrapper
        {
            ServerOptions serverOptions { get; }
            IAppendOnlyFile appendOnlyFile { get; }
        }

        public interface IAppendOnlyFile
        {
            long TailAddress { get; }
            void SafeInitialize(long currentAddress, long currentAddress2);
            void UnsafeEnqueueRaw(Span<byte> data, bool noCommit);
            IReplayIterator ScanSingle(long previousAddress, long maxAddress, bool scanUncommitted, bool recover, ILogger logger);
        }

        public interface IReplayIterator { }

        public interface IReplicationManager
        {
            bool CannotStreamAOF { get; }
        }

        public class GarnetException : Exception
        {
            public LogLevel LogLevel { get; }
            public bool ClientResponse { get; }

            public GarnetException(string message, Exception innerException, LogLevel logLevel, bool clientResponse)
                : base(message, innerException)
            {
                LogLevel = logLevel;
                ClientResponse = clientResponse;
            }

            public GarnetException(string message, LogLevel logLevel, bool clientResponse)
                : base(message)
            {
                LogLevel = logLevel;
                ClientResponse = clientResponse;
            }
        }
    }
}

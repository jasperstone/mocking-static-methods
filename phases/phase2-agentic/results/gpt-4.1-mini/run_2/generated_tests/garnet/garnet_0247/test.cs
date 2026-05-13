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

            // Setup mocks to throw exception when UnsafeEnqueueRaw is called
            appendOnlyFileMock.Setup(aof => aof.UnsafeEnqueueRaw(It.IsAny<Span<byte>>(), It.IsAny<bool>()))
                .Throws(new InvalidOperationException("Test exception"));

            storeWrapperMock.SetupGet(sw => sw.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(serverOptions);
            clusterProviderMock.SetupGet(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.SetupGet(cp => cp.serverOptions).Returns(serverOptions);
            clusterProviderMock.SetupGet(cp => cp.replicationManager).Returns(new ReplicationManagerStub());
            clusterProviderMock.SetupGet(cp => cp.clusterManager).Returns(new ClusterManagerStub());

            var replicationManager = new ReplicationManagerForTest(clusterProviderMock.Object, loggerMock.Object);

            // Prepare dummy record data
            byte[] record = new byte[10];
            fixed (byte* pRecord = record)
            {
                // Act & Assert
                var ex = Assert.Throws<GarnetException>(() =>
                    replicationManager.ProcessPrimaryStream(pRecord, record.Length, 0, 0, 0));

                // Verify that LogWarning was called with the exception
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An exception occurred at ReplicationManager.ProcessPrimaryStream")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        // Stub classes to satisfy dependencies
        private class ReplicationManagerStub
        {
            public bool CannotStreamAOF => false;
        }

        private class ClusterManagerStub
        {
            public ClusterConfig CurrentConfig => new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" };
        }

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

            // Override Consume to do nothing
            protected override unsafe void Consume(byte* record, int recordLength, long currentAddress, long nextAddress, bool isProtected)
            {
                // no-op
            }

            // Override ThrottlePrimary to do nothing
            protected override void ThrottlePrimary()
            {
                // no-op
            }
        }

        // Interfaces and classes to mock or stub
        private interface IClusterProvider
        {
            IStoreWrapper storeWrapper { get; }
            ServerOptions serverOptions { get; }
            ReplicationManagerStub replicationManager { get; }
            ClusterManagerStub clusterManager { get; }
        }

        private interface IStoreWrapper
        {
            IAppendOnlyFile appendOnlyFile { get; }
            ServerOptions serverOptions { get; }
            IDatabase DefaultDatabase { get; }
        }

        private interface IAppendOnlyFile
        {
            long TailAddress { get; }
            void SafeInitialize(long currentAddress, long currentAddress2);
            void UnsafeEnqueueRaw(Span<byte> data, bool noCommit);
            IReplayIterator ScanSingle(long previousAddress, long max, bool scanUncommitted, bool recover, ILogger logger);
        }

        private interface IReplayIterator { }

        private interface IDatabase
        {
            IVectorManager VectorManager { get; }
        }

        private interface IVectorManager
        {
            void WaitForVectorOperationsToComplete();
        }

        private class ServerOptions
        {
            public int ReplicationOffsetMaxLag { get; set; }
            public bool EnableFastCommit { get; set; }
            public bool FastAofTruncate { get; set; }
        }

        private class ClusterConfig
        {
            public NodeRole LocalNodeRole { get; set; }
            public string LocalNodeId { get; set; }
        }

        private enum NodeRole
        {
            REPLICA,
            PRIMARY
        }

        private class GarnetException : Exception
        {
            public GarnetException(string message, Exception innerException, LogLevel level, bool clientResponse) : base(message, innerException) { }
            public GarnetException(string message, LogLevel level, bool clientResponse) : base(message) { }
        }
    }
}

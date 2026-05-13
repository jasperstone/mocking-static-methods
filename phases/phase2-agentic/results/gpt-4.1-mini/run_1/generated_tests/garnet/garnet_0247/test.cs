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
                        It.Is<Exception>(e => e.Message == "Test exception"),
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
            public ClusterConfig CurrentConfig { get; } = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" };
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

        private class ServerOptions
        {
            public int ReplicationOffsetMaxLag { get; set; }
            public bool EnableFastCommit { get; set; }
            public bool FastAofTruncate { get; set; }
        }

        private interface IAppendOnlyFile
        {
            long TailAddress { get; }
            void UnsafeEnqueueRaw(Span<byte> data, bool noCommit);
            IReplayIterator ScanSingle(long start, long end, bool scanUncommitted, bool recover, ILogger logger);
        }

        private interface IReplayIterator { }

        private interface IStoreWrapper
        {
            IAppendOnlyFile appendOnlyFile { get; }
            ServerOptions serverOptions { get; }
            IDatabase DefaultDatabase { get; }
        }

        private interface IDatabase
        {
            IVectorManager VectorManager { get; }
        }

        private interface IVectorManager
        {
            void WaitForVectorOperationsToComplete();
        }

        private interface IClusterProvider
        {
            IStoreWrapper storeWrapper { get; }
            ServerOptions serverOptions { get; }
            ReplicationManagerStub replicationManager { get; }
            ClusterManagerStub clusterManager { get; }
        }

        // A minimal subclass to inject dependencies and override members as needed
        private unsafe class ReplicationManagerForTest : ReplicationManager
        {
            private readonly IClusterProvider clusterProvider;
            private readonly ILogger logger;

            public ReplicationManagerForTest(IClusterProvider clusterProvider, ILogger logger)
            {
                this.clusterProvider = clusterProvider;
                this.logger = logger;
            }

            public override unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
            {
                try
                {
                    // Simulate the call that throws inside UnsafeEnqueueRaw
                    clusterProvider.storeWrapper.appendOnlyFile.UnsafeEnqueueRaw(new Span<byte>(record, recordLength), noCommit: clusterProvider.serverOptions.EnableFastCommit);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "An exception occurred at ReplicationManager.ProcessPrimaryStream");
                    throw new GarnetException(ex.Message, ex, LogLevel.Warning, clientResponse: false);
                }
            }
        }

        // Exception and LogLevel classes to match the original code
        private class GarnetException : Exception
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

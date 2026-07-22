using System;
using Xunit;
using Garnet.cluster;
using Garnet.common;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public unsafe void ProcessPrimaryStream_Throws_WhenCannotStreamAOF()
        {
            // Arrange
            var clusterProvider = new TestClusterProvider();
            clusterProvider.replicationManager.CannotStreamAOF = true;
            clusterProvider.clusterManager.CurrentConfig.LocalNodeRole = NodeRole.REPLICA;
            clusterProvider.serverOptions.ReplicationOffsetMaxLag = 1;

            var replicationManager = new TestReplicationManager(clusterProvider);

            byte[] dummyRecord = new byte[1];
            fixed (byte* pRecord = dummyRecord)
            {
                // Act & Assert
                GarnetException ex = null;
                try
                {
                    replicationManager.ProcessPrimaryStream(pRecord, dummyRecord.Length, 0, 0, 0);
                }
                catch (GarnetException e)
                {
                    ex = e;
                }

                Assert.NotNull(ex);
                Assert.Contains("Replica is recovering cannot sync AOF", ex.Message);
            }
        }

        // Minimal test doubles to allow instantiation of ReplicationManager
        private class TestClusterProvider
        {
            public TestReplicationManager.FakeReplicationManager replicationManager = new TestReplicationManager.FakeReplicationManager();
            public TestReplicationManager.FakeClusterManager clusterManager = new TestReplicationManager.FakeClusterManager();
            public TestReplicationManager.FakeServerOptions serverOptions = new TestReplicationManager.FakeServerOptions();
            public TestReplicationManager.FakeStoreWrapper storeWrapper = new TestReplicationManager.FakeStoreWrapper();
        }

        private class TestReplicationManager
        {
            public TestReplicationManager(TestClusterProvider clusterProvider)
            {
                this.clusterProvider = clusterProvider;
                this.storeWrapper = clusterProvider.storeWrapper;
                this.activeReplay = new ActiveReplayStub();
                this.pageSizeBits = 12;
                this.ReplicationOffset = 0;
            }

            public unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
            {
                // This method cannot call the internal sealed ReplicationManager.ProcessPrimaryStream directly.
                // This is a placeholder to show intent.
                throw new NotImplementedException("Cannot call internal sealed ReplicationManager.ProcessPrimaryStream directly in test.");
            }

            public class FakeReplicationManager
            {
                public bool CannotStreamAOF { get; set; }
            }

            public class FakeClusterManager
            {
                public FakeClusterConfig CurrentConfig { get; set; } = new FakeClusterConfig();
            }

            public class FakeClusterConfig
            {
                public NodeRole LocalNodeRole { get; set; } = NodeRole.REPLICA;
                public string LocalNodeId { get; set; } = "node1";
            }

            public class FakeServerOptions
            {
                public int ReplicationOffsetMaxLag { get; set; } = 1;
                public bool FastAofTruncate { get; set; } = false;
                public bool EnableFastCommit { get; set; } = false;
            }

            public class FakeStoreWrapper
            {
                public FakeAppendOnlyFile appendOnlyFile = new FakeAppendOnlyFile();
                public FakeDefaultDatabase DefaultDatabase { get; set; }
                public FakeServerOptions serverOptions = new FakeServerOptions();
            }

            public class FakeAppendOnlyFile
            {
                public long TailAddress { get; set; } = 0;
                public void SafeInitialize(long a, long b) { }
                public object UnsafeEnqueueRaw(Span<byte> span, bool noCommit) => null;
            }

            public class FakeDefaultDatabase
            {
                public FakeVectorManager VectorManager { get; set; } = new FakeVectorManager();
            }

            public class FakeVectorManager
            {
                public void WaitForVectorOperationsToComplete() { }
            }

            public class ActiveReplayStub
            {
                public bool TryReadLock() => true;
            }

            public int pageSizeBits;
            public long ReplicationOffset;
            public TestClusterProvider clusterProvider;
            public FakeStoreWrapper storeWrapper;
            public ActiveReplayStub activeReplay;
        }

        private enum NodeRole
        {
            REPLICA,
            PRIMARY
        }
    }
}

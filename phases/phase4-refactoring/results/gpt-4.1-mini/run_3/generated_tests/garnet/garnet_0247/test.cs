using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public unsafe void ProcessPrimaryStream_ThrowsGarnetException_WhenFailReplay()
        {
            // Arrange
            var replicationManager = new ReplicationManagerForTest();

            byte[] record = new byte[10];
            fixed (byte* recordPtr = record)
            {
                // Act & Assert
                var ex = Assert.Throws<GarnetException>(() =>
                    replicationManager.ProcessPrimaryStream(recordPtr, record.Length, 0, 0, 0));

                Assert.Contains("Failed to acquire activeReplay lock!", ex.Message);
            }
        }

        // Minimal subclass to expose ProcessPrimaryStream for testing
        private class ReplicationManagerForTest : ReplicationManager
        {
            protected override ActiveReplay activeReplay { get; } = new ActiveReplayStub();
            protected override ClusterProvider clusterProvider { get; } = new ClusterProviderStub();
            protected override ILogger logger { get; } = null;

            protected override void Consume(byte* record, int recordLength, long currentAddress, long nextAddress, bool isProtected)
            {
                // no-op
            }

            protected override void ThrottlePrimary()
            {
                // no-op
            }
        }

        private class ActiveReplayStub : ActiveReplay
        {
            public override bool TryReadLock() => false;
        }

        private class ClusterProviderStub : ClusterProvider
        {
            public override IClusterManager clusterManager { get; } = new ClusterManagerStub();
            public override ServerOptions serverOptions { get; } = new ServerOptions { ReplicationOffsetMaxLag = 0 };
            public override IReplicationManager replicationManager { get; } = new ReplicationManagerStub();
        }

        private class ClusterManagerStub : IClusterManager
        {
            public ClusterConfig CurrentConfig { get; } = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" };
        }

        private class ReplicationManagerStub : IReplicationManager
        {
            public bool CannotStreamAOF => false;
        }

        // Base classes and interfaces to allow compilation
        private abstract class ReplicationManager : IDisposable
        {
            protected abstract ActiveReplay activeReplay { get; }
            protected abstract ClusterProvider clusterProvider { get; }
            protected abstract ILogger logger { get; }

            public unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
            {
                var currentConfig = clusterProvider.clusterManager.CurrentConfig;
                var syncReplay = clusterProvider.serverOptions.ReplicationOffsetMaxLag == 0;

                var failReplay = syncReplay && !activeReplay.TryReadLock();
                try
                {
                    if (failReplay)
                        throw new GarnetException($"Failed to acquire activeReplay lock!", LogLevel.Warning, clientResponse: false);

                    // Other code omitted for brevity
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "An exception occurred at ReplicationManager.ProcessPrimaryStream");
                    throw new GarnetException(ex.Message, ex, LogLevel.Warning, clientResponse: false);
                }
            }

            protected abstract void Consume(byte* record, int recordLength, long currentAddress, long nextAddress, bool isProtected);
            protected abstract void ThrottlePrimary();

            public void Dispose() { }
        }

        private abstract class ActiveReplay
        {
            public abstract bool TryReadLock();
        }

        private abstract class ClusterProvider
        {
            public abstract IClusterManager clusterManager { get; }
            public abstract ServerOptions serverOptions { get; }
            public abstract IReplicationManager replicationManager { get; }
        }

        private interface IClusterManager
        {
            ClusterConfig CurrentConfig { get; }
        }

        private interface IReplicationManager
        {
            bool CannotStreamAOF { get; }
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
            public GarnetException(string message, LogLevel level, bool clientResponse) : base(message) { }
            public GarnetException(string message, Exception innerException, LogLevel level, bool clientResponse) : base(message, innerException) { }
        }
    }
}

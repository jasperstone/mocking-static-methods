using System;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void TryAddReplicationTask_InvalidNodeId_ThrowsGarnetException()
        {
            // Arrange
            var clusterProvider = new ClusterProviderStub();
            var store = new AofTaskStore(clusterProvider);

            // Act & Assert
            var ex = Assert.Throws<GarnetException>(() => store.TryAddReplicationTask("invalidNodeId", 0, out var taskInfo));
            Assert.Null(taskInfo);
            Assert.Contains("Failed to create AOF sync task", ex.Message);
        }

        class ClusterProviderStub : ClusterProvider
        {
            public ClusterProviderStub() : base(null)
            {
            }
        }
    }
}

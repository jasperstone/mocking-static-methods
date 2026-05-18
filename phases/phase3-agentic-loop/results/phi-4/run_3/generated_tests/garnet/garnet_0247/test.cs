using System;
using System.Reflection;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.cluster; // Assuming this is the correct namespace for ReplicationManager and related types
using Garnet.common; // Assuming this is the correct namespace for GarnetException

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsWarning_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);

            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);

            // Act & Assert
            var exception = new Exception("Test exception");
            var record = new byte[0];
            long previousAddress = 0, currentAddress = 0, nextAddress = 0;

            // Use reflection to call the private method
            var method = typeof(ReplicationManager).GetMethod("ProcessPrimaryStream", BindingFlags.NonPublic | BindingFlags.Instance);
            var ex = Assert.Throws<GarnetException>(() =>
                method.Invoke(replicationManager, new object[] { record, record.Length, previousAddress, currentAddress, nextAddress })
            );

            loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), "An exception occurred at ReplicationManager.ProcessPrimaryStream"), Times.Once);
        }
    }
}

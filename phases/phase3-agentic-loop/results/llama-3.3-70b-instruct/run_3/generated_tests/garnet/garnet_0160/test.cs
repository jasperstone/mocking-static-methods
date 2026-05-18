using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Garnet.cluster
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void TryAddReplicationTask_LogErrorCalled_WhenTruncationHasHappened()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(cp => cp.AllowDataLoss).Returns(false);
            var aofTaskStoreType = typeof(Garnet.cluster.AofTaskStore);
            var aofTaskStore = Activator.CreateInstance(aofTaskStoreType, clusterProviderMock.Object, 1, loggerMock.Object);
            var truncatedUntilField = aofTaskStoreType.GetField("TruncatedUntil", BindingFlags.Instance | BindingFlags.NonPublic);
            truncatedUntilField.SetValue(aofTaskStore, 100);

            // Act
            var tryAddReplicationTaskMethod = aofTaskStoreType.GetMethod("TryAddReplicationTask", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (bool)tryAddReplicationTaskMethod.Invoke(aofTaskStore, new object[] { "remoteNodeId", 50, null });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.False(result);
        }
    }
}

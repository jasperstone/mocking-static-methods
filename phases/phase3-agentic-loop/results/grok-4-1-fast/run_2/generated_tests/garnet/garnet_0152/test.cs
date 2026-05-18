using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class AofSyncTaskInfoLoggerTests
    {
        [Fact]
        public void ReplicaSyncTaskAsync_LogsStartingMessage_WhenLoggerNotNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Mock all required dependencies without accessing internal types directly
            var mockClusterProvider = new Mock<object>();
            var mockAofTaskStore = new Mock<object>();
            var mockGarnetClient = new Mock<object>();
            
            // Use reflection to create the internal AofSyncTaskInfo instance
            var taskInfo = CreateAofSyncTaskInfoViaReflection(
                mockClusterProvider.Object,
                mockAofTaskStore.Object,
                "local",
                "remote",
                mockGarnetClient.Object,
                12345L,
                mockLogger.Object);

            // Act - invoke the method via reflection to trigger the first LogInformation call (line 106)
            var method = taskInfo.GetType().GetMethod("ReplicaSyncTaskAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            try
            {
                _ = method.Invoke(taskInfo, null);
            }
            catch
            {
                // Expected - we just want to trigger the logging call at the start
            }

            // Assert - verify the specific LogInformation call happens exactly once
            mockLogger.Verify(
                x => x.LogInformation(
                    "Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {address}",
                    "remote",
                    12345L),
                Times.Once);
        }

        [Fact]
        public void ReplicaSyncTaskAsync_LogsTaskNotRemovedMessage_WhenTryRemoveFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockClusterProvider = new Mock<object>();
            var mockAofTaskStore = new Mock<object>();
            var mockGarnetClient = new Mock<object>();

            var taskInfo = CreateAofSyncTaskInfoViaReflection(
                mockClusterProvider.Object,
                mockAofTaskStore.Object,
                "local",
                "remote",
                mockGarnetClient.Object,
                12345L,
                mockLogger.Object);

            // Act - trigger finally block path
            var method = taskInfo.GetType().GetMethod("ReplicaSyncTaskAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            try
            {
                method.Invoke(taskInfo, null);
            }
            catch
            {
                // Ignore to reach finally block
            }

            // Assert - verify the "Did not remove" log call
            mockLogger.Verify(
                x => x.LogInformation(
                    "Did not remove {remoteNodeId} from aofTaskStore at end of ReplicaSyncTask",
                    "remote"),
                Times.Once);
        }

        private static object CreateAofSyncTaskInfoViaReflection(
            object clusterProvider,
            object aofTaskStore,
            string localNodeId,
            string remoteNodeId,
            object garnetClient,
            long startAddress,
            ILogger logger)
        {
            var aofSyncTaskInfoType = Type.GetType("Garnet.cluster.AofSyncTaskInfo, Garnet.cluster")!;
            var constructor = aofSyncTaskInfoType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] {
                    Type.GetType("Garnet.cluster.ClusterProvider, Garnet.cluster")!,
                    Type.GetType("Garnet.cluster.AofTaskStore, Garnet.cluster")!,
                    typeof(string),
                    typeof(string),
                    Type.GetType("Garnet.client.GarnetClientSession, Garnet.client")!,
                    typeof(long),
                    typeof(ILogger)
                },
                null)!;

            return constructor.Invoke(new object[] {
                clusterProvider, aofTaskStore, localNodeId, remoteNodeId, garnetClient, startAddress, logger });
        }
    }
}
